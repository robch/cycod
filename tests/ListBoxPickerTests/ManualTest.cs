using System;
using ConsoleGui.Controls;

#if MANUAL_TEST
class TestPickerManual
{
    static void Main()
    {
        Console.WriteLine("Simple ListBoxPicker Demo");
        Console.WriteLine("========================");
        Console.WriteLine();
        Console.WriteLine("Please select your favorite color:");
        Console.WriteLine();
        
        var colors = new[] { "Red", "Green", "Blue", "Yellow", "Purple", "Orange" };
        var selected = ListBoxPicker.PickString(colors);
        
        Console.WriteLine();
        if (selected != null)
        {
            Console.WriteLine($"You selected: {selected}");
        }
        else
        {
            Console.WriteLine("Selection cancelled (pressed Escape)");
        }
    }
}
#endif
