using System;
using System.Diagnostics;
using System.Threading;

public class MultiStepTicketQueue<T> where T : struct
{
    private const int MAX_TICKETS = 1024;
    private const byte START_STEP = 0;
    private const byte DISPOSED_STEP = 255;
    
    private readonly byte[] _ticketSteps = new byte[MAX_TICKETS];
    private long _nextTicket = 0;
    
    public MultiStepTicketQueue()
    {
        Array.Fill(_ticketSteps, START_STEP);
    }
    
    public TicketGuard CreateTicketGuard()
    {
        var ticket = Interlocked.Increment(ref _nextTicket) - 1;
        var slot = (int)(ticket % MAX_TICKETS);
        
        Debug.Assert(_ticketSteps[slot] == START_STEP);
        Volatile.Write(ref _ticketSteps[slot], (byte)(START_STEP + 1));
        
        return new TicketGuard(this, ticket);
    }
    
    public byte AdvanceToStep(long ticket, byte step)
    {
        return AdvanceStepInternal(ticket, step);
    }
    
    private byte AdvanceStepInternal(long ticket, byte newStep)
    {
        var slot = (int)(ticket % MAX_TICKETS);
        var prevSlot = ticket == 0 ? MAX_TICKETS - 1 : (int)((ticket - 1) % MAX_TICKETS);
        
        var loopCount = 0;
        while (true)
        {
            var unblocked = (ticket == 0) || 
                            (Volatile.Read(ref _ticketSteps[prevSlot]) > newStep) ||
                            (newStep == DISPOSED_STEP && Volatile.Read(ref _ticketSteps[prevSlot]) == DISPOSED_STEP);
                            
            if (unblocked) break;
            
            if (loopCount++ == 100)
            {
                Thread.Yield();
                loopCount = 0;
            }
        }
        
        Volatile.Write(ref _ticketSteps[slot], newStep);
        return newStep;
    }
    
    public void DisposeTicket(long ticket)
    {
        AdvanceStepInternal(ticket, DISPOSED_STEP);
        
        var prevSlot = ticket == 0 ? MAX_TICKETS - 1 : (int)((ticket - 1) % MAX_TICKETS);
        Debug.Assert(Volatile.Read(ref _ticketSteps[prevSlot]) == DISPOSED_STEP || ticket == 0);
        Volatile.Write(ref _ticketSteps[prevSlot], START_STEP);
    }
    
    public struct TicketGuard : IDisposable
    {
        private MultiStepTicketQueue<T>? _queue;
        private readonly long _ticket;
        
        public TicketGuard(MultiStepTicketQueue<T> queue, long ticket)
        {
            _queue = queue;
            _ticket = ticket;
        }
        
        public byte AdvanceToStep(byte step) => _queue?.AdvanceToStep(_ticket, step) ?? 0;
        
        public void Dispose()
        {
            if (_queue != null)
            {
                _queue.DisposeTicket(_ticket);
                _queue = null;
            }
        }
    }
}