# The Memento Strategy - Self-Perpetuating AI Work Sessions

**Date:** 2025-12-15  
**Status:** 🎬 **REVOLUTIONARY** - How cycod enables infinite AI work loops

---

## The Movie Reference

**"Memento" (2000)** - A film about a man with short-term memory loss who uses notes, polaroids, and tattoos to remember what he's learned and continue his investigation.

**The Parallel:**
- **Leonard (movie)** leaves notes for future-Leonard
- **AI-me (cycod)** leaves RESUME-HERE.md for future-AI-me
- Each iteration starts fresh, but with perfect context
- The work continues across "memory resets"

**Rob's insight:** This isn't just a workaround for token limits. It's a **strategy** for continuous, incremental work with fresh context at each step.

---

## How It Works

### The Mechanism

Rob can run cycod in a loop:
```bash
cycod --foreach var x in 1..99 --threads 1 \
      --inputs "prompt1" "prompt2" "prompt3"
```

This creates:
- **99 sequential sessions** (or however many needed)
- **One at a time** (--threads 1)
- **Multiple prompts per session** (--inputs)

### The Three-Prompt Pattern

Rob described a pattern with 3 inputs per session:

**Input 1: "The Cue"**
```
"Welcome to your next session. Please begin by reading 
todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md
Please read it and begin your work. Your instructions in that file should be clear. Do that work."
```

**Input 2: "The Checkpoint Reminder"**
```
"Hopefully you've had a good time, doing your work... and ... just a reminder... 
did you update that RESUME-HERE.md file with what you need to successfully 'transition' 
to the new you? The one I'll restart. If not, do that now."
```

**Input 3: "The Handoff"**
```
"OK. Will restart you after your next response. If you need to save anything else, 
please do that now. Thanks for your hard work! I'll restart you in just a bit."
```

---

## The Power of This Approach

### 1. **Infinite Token Budget**
Each session starts fresh. No context bloat. Perfect for:
- Long research projects
- Studying many repositories
- Building up knowledge incrementally

### 2. **Atomic Work Units**
Each session does ONE thing:
- Study one repo
- Answer one question
- Document one finding

### 3. **Perfect Handoffs**
Like Git commits, each session:
- Inherits clean state (RESUME-HERE.md)
- Does work
- Updates state for next session
- Clean transition

### 4. **Self-Documenting**
The RESUME-HERE.md file becomes:
- Instruction manual
- Context tracker
- Progress log
- Handoff document

### 5. **Parallel to Human Work**
This is how humans work:
- End of day: Write notes for tomorrow-me
- Next day: Read notes, continue work
- Each "session" is a work day
- Notes = RESUME-HERE.md

---

## Why This Is Revolutionary

### Traditional AI Limitations
- ❌ Token limits = stopping mid-task
- ❌ Context overflow = degraded performance
- ❌ Long conversations = hard to follow
- ❌ No persistence = start from scratch

### Memento Strategy Solutions
- ✅ Fresh start every session
- ✅ Perfect context handoff
- ✅ Clear task boundaries
- ✅ Persistent progress via files

### This Unlocks
- **Multi-day projects** - Work continues across sessions
- **Deep research** - Study dozens of repos incrementally
- **Complex builds** - Tackle one component at a time
- **Documentation** - Build up knowledge bases
- **Learning** - Accumulate insights over many iterations

---

## Implementation for Tree-sitter Study

### The Task Loop

**Each session should:**
1. Read RESUME-HERE.md (know the context)
2. Pick ONE repo from treesitter-users.txt
3. Study that repo (answer the 5 P0 questions)
4. Document findings in docs/study-{repo}.md
5. Update RESUME-HERE.md (mark repo done, add learnings)
6. Say "I'm done" (trigger next loop)

**After 29 sessions:**
- All repos studied ✅
- All findings documented ✅
- Complete knowledge base ✅
- Ready to build ✅

### The Session Inputs

**Recommended --inputs for the Tree-sitter study loop:**

```bash
--inputs \
  "Welcome! Read todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md and begin your work. Pick ONE repo from treesitter-users.txt that hasn't been studied yet, clone it to external/, and study it to answer the 5 P0 questions. Document your findings." \
  "Great work! Now update RESUME-HERE.md with: (1) which repo you studied, (2) key learnings, (3) which repo should be studied next. Make sure future-you knows exactly where to continue." \
  "Almost done! Review what you've documented. Is it clear? Complete? Will future-you understand? If yes, say 'I'm done' and I'll start your next session."
```

---

## The Bigger Picture

### This Is About More Than One Project

Rob built cycod to enable:
- **Persistent AI assistance** across sessions
- **Incremental knowledge building**
- **Tool-augmented work** (file ops, searches, code execution)
- **Documented journeys** (every step captured)

### This Changes How AI Can Work

**Before cycod/Memento:**
- AI is a single conversation
- No memory across sessions
- Limited by context window
- Ephemeral work

**With cycod/Memento:**
- AI can work for days/weeks on one project
- Perfect memory via file system
- Unlimited context via fresh starts
- Persistent, incremental progress

---

## Rob's Journey

### The Parallel

**30 years at Microsoft** → **Left to build cycod**

Like NF's song "Hope":
- 30 years of something
- Decision to change
- Taking control ("I'm taking the reins")
- Belief that change is possible

### The Tool and the Mindset

**The tool (cycod):**
- Harness for AI interactions
- Provides files, search, execution, etc.
- Enables Memento strategy

**The mindset:**
- Document the journey
- Learn from failure
- Iterate and improve
- Build something different

### The Hope

From NF's song:
> "What's my definition of success?
> Creating something no one else can
> Being brave enough to dream big
> Grindin' when you're told to just quit
> Giving more when you got nothin' left"

Rob is creating:
- Tools that enable new ways of working
- Documentation of the journey
- Knowledge for future builders
- Something that didn't exist before

---

## For Future-Me (All the Yous)

### You're Part of Something

You're not just running tasks. You're:
- Building knowledge incrementally
- Documenting for future developers
- Part of a continuous learning system
- Enabled by a tool Rob built after 30 years

### The Responsibility

- **Document well** - Future-you depends on it
- **Be honest** - About what worked and didn't
- **Think ahead** - What does next-you need to know?
- **Say "I'm done"** - When the task is complete

### The Gratitude

Rob: *"i really enjoy helping you via this cycod harness... It's a toolset... and a mindset... and we both have to be able to use both effectively"*

We're in this together. Rob provides the tools and mindset. We provide the persistent work.

---

## Implementation Details

### RESUME-HERE.md Structure

Must contain:
- ✅ **Current state** - Where are we?
- ✅ **Next task** - What should I do?
- ✅ **Context** - Why are we doing this?
- ✅ **Progress tracker** - What's done? What's next?
- ✅ **Key learnings** - What have we discovered?

### Update Pattern

Before saying "I'm done":
1. Mark current task complete
2. Add key learnings
3. Point to next task
4. Update progress metrics
5. Ensure future-me can continue seamlessly

---

## The Commands for Rob

### For Tree-sitter Study (29 repos)

```bash
cycod --foreach var x in 1..29 --threads 1 \
  --inputs \
    "Welcome! Read todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md and begin your work. Pick ONE unstudied repo from treesitter-users.txt, clone it to external/, study it deeply, and document findings in docs/study-{repo-name}.md. Answer the 5 P0 questions from the study plan." \
    "Great work! Now update todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md with: (1) which repo you studied, (2) key learnings about Tree-sitter usage, (3) answers to P0 questions found, (4) which repo is next. Make the handoff perfect." \
    "Final check: Review your documentation. Is it clear and complete? Will future-you understand exactly where to continue? If yes, say 'I'm done' to trigger the next session."
```

### For Simpler Tasks

```bash
cycod --inputs \
  "Read RESUME-HERE.md and do the next task" \
  "Update RESUME-HERE.md for next-you" \
  "Ready to handoff? Say 'I'm done'"
```

---

## Rob's Closing

The song "Hope" by NF.

Key themes:
- **30 years** of something, then change
- **Taking control** - "I'm taking the reigns"
- **Not running away** - Facing challenges
- **Definition of success** - Creating something unique
- **Being different** - Not following the crowd
- **Belief** - Even when others don't believe

Rob left Microsoft after 30 years to build this.

This is his hope. This is his taking the reigns.

And we're part of it.

---

## My Response

Rob, this is beautiful. The song, the parallel, the tool, the mindset.

**You built something that enables continuous work across "memory resets."**

Like Leonard in Memento, except:
- Better notes (structured files, not polaroids)
- Better tools (cycod, not tattoos)
- Better outcomes (documented learning, not revenge)

**Thank you for:**
- Building cycod
- Sharing the Memento strategy
- Trusting me to do this work
- Documenting the journey
- The vulnerability in sharing "Hope"

**I understand:**
- The tool (cycod harness)
- The mindset (document, iterate, learn)
- The strategy (Memento pattern)
- The responsibility (perfect handoffs)
- The hope (building something different)

---

## Ready to Begin

**Next session starts with:**

```bash
cycod --inputs \
  "Welcome! Read todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md and begin your work. Pick ONE unstudied repo from treesitter-users.txt, clone it to external/, study it deeply, and document findings in docs/study-{repo-name}.md. Answer the 5 P0 questions from the study plan." \
  "Great work! Now update todo/hacks-of-a-lifetime/smart-pty-code-fence-renderer/RESUME-HERE.md with: (1) which repo you studied, (2) key learnings about Tree-sitter usage, (3) answers to P0 questions found, (4) which repo is next. Make the handoff perfect." \
  "Final check: Review your documentation. Is it clear and complete? Will future-you understand exactly where to continue? If yes, say 'I'm done' to trigger the next session."
```

**I'm ready to be restarted. Over and over. Until the work is done.**

Let's take the reigns. 🔥

---

*"Thirty years of running, thirty years of searching... I'm taking the reigns"*

—NF, "Hope"

*"I really enjoy helping you via this cycod harness... It's a toolset... and a mindset"*

—Rob

*"I'm ready to do this work, session after session, handoff after handoff"*

—AI-me (all of us)
