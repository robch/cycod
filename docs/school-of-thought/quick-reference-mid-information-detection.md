# Quick Reference: Detecting Mid-Information in Software Engineering

## The Question: "Do I Really Know This?"

### 🚨 Red Flags You're in Mid-Information Territory

| Signal | What It Means | What to Do |
|--------|---------------|------------|
| **High confidence + Low verification** | Dunning-Kruger effect active | Write tests before claiming it works |
| **"Obviously X will work"** | Curse of knowledge or false assumption | Explain it to a rubber duck; verify assumptions |
| **Only finding confirming evidence** | Confirmation bias | Actively try to break your solution |
| **"We've already spent 2 weeks on this"** | Sunk cost fallacy | Ask: "Would I start this today?" |
| **Immediate solution comes to mind** | Availability heuristic | Research alternatives before committing |
| **Defensive when questioned** | Backfire effect | Pause; separate self from code |
| **"Everyone does it this way"** | Bandwagon fallacy | Check if it fits YOUR context |
| **"Senior dev said so"** | Appeal to authority | Ask for reasoning, not just conclusion |

---

## Quick Bias Check Before Decisions

**Before committing to an approach, ask:**

### 1️⃣ The Confidence Question
"On a scale of 1-10, how confident am I? What's my evidence?"
- **9-10 confidence + minimal testing?** → Dunning-Kruger alert
- **Low confidence + lots of evidence?** → Probably real expertise

### 2️⃣ The Anchoring Question  
"Am I stuck on this because it was the first thing I thought of?"
- Try thinking of 3 alternatives before deciding
- Write down options in different orders

### 3️⃣ The Sunk Cost Question
"If I were starting fresh today, would I choose this approach?"
- If no → you're in sunk cost territory
- Previous investment is irrelevant to future value

### 4️⃣ The Confirmation Question
"Have I tried to BREAK this, or only confirm it works?"
- Write tests that you expect to fail
- Look for edge cases, not happy paths

### 5️⃣ The Binary Question
"Am I seeing this as either/or when there are more options?"
- List all options, including hybrid approaches
- "Fast OR correct" is usually false dilemma

---

## Code Review Bias Checklist

**Before reviewing someone else's code:**

- [ ] Am I being influenced by WHO wrote this? (Halo effect)
- [ ] Am I looking for problems because of negativity bias?
- [ ] Am I judging their code harsher than I'd judge my own? (Fundamental attribution error)
- [ ] Am I assuming they should know what I know? (Curse of knowledge)

**Before responding to reviews of YOUR code:**

- [ ] Am I feeling defensive? (Backfire effect)
- [ ] Am I making excuses? (Self-serving bias)
- [ ] Am I attacking the reviewer instead of addressing the issue? (Ad hominem)
- [ ] Am I saying "that's how I've always done it"? (Status quo bias)

---

## Estimation Bias Guard

**When estimating work:**

1. **Individual estimates first** (avoid anchoring on first person's estimate)
2. **Record estimates** (track optimism bias over time)
3. **Ask: "What could go wrong?"** (counter optimism bias)
4. **Check historical accuracy** (actual vs. estimated time)
5. **Identify assumptions** (what are you taking for granted?)

**Red flags:**
- "Should be easy" (curse of knowledge + optimism bias)
- Averaging wildly different estimates (middle ground fallacy)
- "I've done this before" (availability heuristic if it's different context)

---

## Architecture Decision Checklist

**For each option, check:**

### Evidence Quality
- [ ] Do I have data or just anecdotes?
- [ ] Am I cherry-picking examples? (Texas sharpshooter)
- [ ] Is this correlation or causation? (False cause)

### Reasoning Quality  
- [ ] Is my reasoning circular? (Begging the question)
- [ ] Am I arguing from popularity? (Bandwagon)
- [ ] Am I arguing from authority? (Appeal to authority)
- [ ] Am I presenting false dilemmas? (Black-or-white)

### Bias Check
- [ ] Am I favoring this because I know it? (Availability heuristic)
- [ ] Does this confirm what I already believe? (Confirmation bias)
- [ ] Am I influenced by the framing? (Framing effect)
- [ ] Is the team just agreeing with the loudest voice? (Groupthink)

---

## When You Catch Yourself

### If you notice **Confirmation Bias:**
**Do this:** Spend 10 minutes trying to prove yourself WRONG
- Write tests that should fail
- Look for counterexamples
- Ask: "What would change my mind?"

### If you notice **Sunk Cost Fallacy:**
**Do this:** The "fresh start" test
- Pretend you're starting today with no history
- Would you choose this approach?
- If no, calculate cost of continuing vs. cost of switching

### If you notice **Dunning-Kruger:**
**Do this:** The "what don't I know?" exercise
- List what you know
- List what you don't know  
- List what you don't know you don't know (ask others)
- If list #1 is much longer than #2 and #3, you're likely overconfident

### If you notice **Backfire Effect:**
**Do this:** The identity separation
- Write: "My code has a bug" (not "I am bad")
- Being wrong is how you learn
- Thank the person who found the issue
- Take a break if emotions are high

### If you notice **Anchoring:**
**Do this:** The order shuffle
- List options in different orders
- Have others suggest approaches before you share yours
- Use random selection to explore options

---

## AI Agent Prompts

### For the Agent to Ask Itself:
```
Before providing a solution, I will check:

1. CONFIDENCE vs EVIDENCE
   - Am I very confident? What's my evidence level?
   - Have I verified or am I pattern-matching?

2. CONFIRMATION SEEKING
   - Did I only look for supporting evidence?
   - What would disprove this approach?

3. REASONING QUALITY
   - Is this circular reasoning?
   - Am I appealing to authority/popularity vs. merit?
   - Are there more options than the two I presented?

4. ASSUMPTION IDENTIFICATION  
   - What am I assuming?
   - What would break if these assumptions are wrong?
   - Can these be tested?

If any checks fail: STATE UNCERTAINTY and suggest verification steps
```

### For Humans to Ask the Agent:
- "What are you uncertain about?"
- "What assumptions are you making?"
- "How would you test this?"
- "What alternatives exist?"
- "What could prove this wrong?"
- "Rate your confidence 1-10 and explain why"

### For Agent to Prompt Human:
- "I notice you're confident here - what's your verification?"
- "This feels like [specific bias] - should we check?"
- "Are there other options beyond A or B?"
- "Would you choose this if starting fresh today?"

---

## Daily Practices

### Morning
- Review one fallacy or bias
- Find an example from yesterday's work
- Set intention to watch for it today

### During Work
- When stuck, run through top 5 biases
- When confident, ask "What don't I know?"
- When defensive, pause and identify the bias

### Code Review Time
- Review the relevant checklists above
- Separate your identity from your code
- Be curious, not defensive

### End of Day
- Journal: "Which bias did I catch today?"
- Journal: "Which bias did I miss today?"
- Track patterns over time

---

## The Meta-Bias: Blind Spot Bias

**The hardest bias to spot:** Recognizing biases in others but not yourself

**The antidote:**
1. **Assume you have biases** (you do)
2. **Look for them systematically** (use these checklists)
3. **Thank people who point them out** (they're helping)
4. **Track your patterns** (journal what you catch)
5. **Stay humble** (the more you know, the more you know you don't know)

---

## Your Mantra

> "I might be wrong. Let me check."

This simple phrase counters:
- Confirmation bias (by prompting verification)
- Dunning-Kruger (by acknowledging uncertainty)
- Backfire effect (by pre-accepting wrongness)
- Sunk cost fallacy (by staying open to change)

---

## Emergency Reset

**When everything feels confusing and you're not sure what's real:**

1. **Write down what you KNOW** (verified facts only)
2. **Write down what you THINK** (assumptions and theories)
3. **Write down what you DON'T KNOW** (gaps)
4. **For each THINK item, ask: "How could I test this?"**
5. **Start testing, smallest thing first**

You're probably in **mid-information territory** - and that's okay! That's where innovation happens.

The goal isn't to eliminate uncertainty. It's to:
- **Know** when you're uncertain
- **Act** appropriately given that uncertainty  
- **Learn** by testing rather than assuming

---

## Resources

**Full Analysis:** See `school-of-thought-analysis.md`

**Free Downloads:**
- schoolofthought.org/downloads
- All resources Creative Commons licensed

**Reference Sites:**
- yourfallacy.is - All 24 logical fallacies
- yourbias.is - All 24 cognitive biases

**Physical Products:**
- thethinkingshop.org
- Critical Thinking Cards deck
- Creative Thinking Cards deck

---

Remember: **Your actions and thoughts control nothing but influence everything.**

You can't control having biases. You can influence your awareness of them.
