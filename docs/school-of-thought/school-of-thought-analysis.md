# School of Thought Resources - Analysis for Software Engineering & AI Agents

## Overview

The School of Thought is a 501c3 non-profit organization providing free educational resources on critical thinking, logical fallacies, and cognitive biases. All resources are available under Creative Commons licenses.

**Key Resources:**
- **Logical Fallacies Poster** - 24 common fallacies
- **Cognitive Biases Poster** - 24 common biases
- **Critical Thinking Cards** - 54 cards (24 fallacies + 24 biases + 3 game cards + 3 call-out cards)
- **Creative Thinking Cards** - 52 brainstorming tools

**Websites:**
- yourfallacy.is - Logical fallacies
- yourbias.is - Cognitive biases
- schoolofthought.org - Main organization site
- thethinkingshop.org - Purchase physical products or download free PDFs

---

## The 24 Logical Fallacies

### 1. **Strawman**
Misrepresenting someone's argument to make it easier to attack.

**Software Engineering Context:**
- Dismissing a proposed refactoring by exaggerating its scope
- "You want to change this function? So you want to rewrite the entire codebase?"

**Detection Trigger:** When someone restates your proposal in more extreme terms

---

### 2. **False Cause** (Post hoc ergo propter hoc)
Presuming that a relationship between things means one causes the other.

**Software Engineering Context:**
- "We deployed on Friday and the server crashed on Saturday, so Friday deployments cause crashes"
- Confusing correlation with causation in performance metrics

**Detection Trigger:** When you see two events happen together and immediately assume causation

---

### 3. **Slippery Slope**
Asserting that if we allow A, then Z will consequently happen too.

**Software Engineering Context:**
- "If we add one feature flag, we'll end up with thousands and the code will be unmaintainable"
- Opposing any technical debt without evidence it will spiral

**Detection Trigger:** Arguments that jump from a small change to catastrophic consequences without showing the intermediate steps

---

### 4. **Ad Hominem**
Attacking the person instead of engaging with their argument.

**Software Engineering Context:**
- "Why should we listen to the junior dev's suggestion?"
- Dismissing code reviews based on who submitted them, not the code quality

**Detection Trigger:** When criticism focuses on the person rather than the idea/code

---

### 5. **Appeal to Emotion**
Manipulating emotional response instead of using valid arguments.

**Software Engineering Context:**
- "If we don't ship this feature, we'll disappoint our users!" (without data)
- Using fear of failure to rush decisions without proper analysis

**Detection Trigger:** Arguments that make you feel strongly but provide little logical substance

---

### 6. **The Fallacy Fallacy**
Presuming that because an argument is poorly made, the conclusion is wrong.

**Software Engineering Context:**
- A junior dev makes a syntax error while proposing a good architectural idea, so the idea is dismissed
- Rejecting a valid solution because the explanation was unclear

**Detection Trigger:** When you dismiss an idea entirely because of how it was presented

---

### 7. **Tu Quoque** (Appeal to Hypocrisy)
Turning criticism back on the accuser instead of addressing it.

**Software Engineering Context:**
- "Your code has bugs too!" in response to a code review
- "You don't write tests either!" instead of addressing the testing concern

**Detection Trigger:** Responses that deflect by pointing out the critic's similar behavior

---

### 8. **Personal Incredulity**
Saying something isn't true because you find it hard to understand.

**Software Engineering Context:**
- "I don't understand how async/await works, so it must be bad"
- Rejecting functional programming concepts because they're unfamiliar

**Detection Trigger:** "I can't see how this could work, therefore it doesn't"

---

### 9. **Special Pleading**
Moving goalposts or creating exceptions when claims are shown false.

**Software Engineering Context:**
- "The test failed because the environment wasn't set up correctly" (repeatedly)
- Constantly finding excuses for why your approach didn't work instead of reconsidering

**Detection Trigger:** Repeated exceptions and new conditions added after failures

---

### 10. **Loaded Question**
Asking a question with an assumption built in that can't be answered without appearing guilty.

**Software Engineering Context:**
- "Have you stopped writing bad code?"
- "Are you still ignoring best practices?"

**Detection Trigger:** Questions that presuppose something negative regardless of the answer

---

### 11. **Burden of Proof**
Claiming others must disprove you rather than you proving your claim.

**Software Engineering Context:**
- "This approach is better unless you can prove it isn't"
- "Show me evidence that microservices won't work here"

**Detection Trigger:** Demanding others disprove your claim instead of providing evidence

---

### 12. **Ambiguity**
Using double meanings or language ambiguities to mislead.

**Software Engineering Context:**
- Vague ticket descriptions that can be interpreted multiple ways
- "This will be done soon" (what does "soon" mean?)

**Detection Trigger:** Terms that could mean multiple things but are presented as clear

---

### 13. **The Gambler's Fallacy**
Believing that past random events affect future independent events.

**Software Engineering Context:**
- "The last three deployments worked fine, so this one will too" (without testing)
- "We haven't had a major bug in weeks, we're due for one"

**Detection Trigger:** Expecting patterns in independent random events

---

### 14. **Bandwagon**
Arguing something is valid because it's popular.

**Software Engineering Context:**
- "Everyone is using React, so we should too" (without evaluating fit)
- "Most developers prefer X, so it must be better"

**Detection Trigger:** "Everyone is doing it" as the primary justification

---

### 15. **Appeal to Authority**
Using an authority's position instead of an actual argument.

**Software Engineering Context:**
- "The senior architect said so, therefore it's correct"
- "This framework is used by Google, so we should use it"

**Detection Trigger:** Position/prestige as the main argument rather than evidence

---

### 16. **Composition/Division**
Assuming what's true for a part applies to the whole, or vice versa.

**Software Engineering Context:**
- "This function is fast, so the whole system will be fast"
- "The system is slow, so every component must be slow"

**Detection Trigger:** Generalizing from part to whole without verification

---

### 17. **No True Scotsman**
Making purity appeals to dismiss criticisms.

**Software Engineering Context:**
- "No real programmer would write code like that"
- "That's not real TDD if you wrote the test second"

**Detection Trigger:** Moving definitions to exclude inconvenient examples

---

### 18. **Genetic Fallacy**
Judging something based on its origin rather than its merits.

**Software Engineering Context:**
- Dismissing open source solutions because they're "not enterprise"
- Rejecting ideas from certain companies/sources automatically

**Detection Trigger:** Origin or source as the basis for judgment

---

### 19. **Black-or-White** (False Dilemma)
Presenting only two options when more exist.

**Software Engineering Context:**
- "We either rewrite everything or keep the legacy code as-is"
- "We can have it fast or correct, not both"

**Detection Trigger:** "Either/or" statements that ignore middle ground

---

### 20. **Begging the Question** (Circular Reasoning)
The conclusion is included in the premise.

**Software Engineering Context:**
- "This code is good because it's well-written, and it's well-written because it's good code"
- "We need this feature because users want it, and users want it because they need it"

**Detection Trigger:** Arguments that loop back on themselves

---

### 21. **Appeal to Nature**
Arguing something is good/valid because it's "natural."

**Software Engineering Context:**
- "Manual processes are more natural than automation"
- "Imperative code is more natural than functional code"

**Detection Trigger:** "Natural" as justification without other reasoning

---

### 22. **Anecdotal Evidence**
Using personal experience instead of sound evidence/statistics.

**Software Engineering Context:**
- "I once saw this pattern fail, so it's always bad"
- "In my experience, X always causes problems" (without broader data)

**Detection Trigger:** Single examples used to dismiss statistical evidence

---

### 23. **Texas Sharpshooter**
Cherry-picking data to suit an argument.

**Software Engineering Context:**
- Only running tests that pass and ignoring failing ones
- Showing metrics only from successful sprints

**Detection Trigger:** Selective data presentation that supports a narrative

---

### 24. **Middle Ground**
Assuming the compromise between two extremes must be true.

**Software Engineering Context:**
- "One person says 2 weeks, another says 2 months, so the truth must be 5 weeks"
- Averaging estimates without understanding the underlying complexity

**Detection Trigger:** Automatic averaging or compromise without evidence

---

## The 24 Cognitive Biases

### 1. **Anchoring**
The first thing you judge influences your judgment of all that follows.

**Software Engineering Context:**
- First estimate becomes the anchor, making later estimates bias toward it
- Initial code structure influences all future changes

**Detection Trigger:** When initial information disproportionately affects later thinking
**Mitigation:** Deliberately consider information in different orders

---

### 2. **Sunk Cost Fallacy**
Irrationally clinging to things that have already cost you something.

**Software Engineering Context:**
- "We've spent 6 months on this approach, we can't switch now"
- Continuing with a bad framework because you've already learned it

**Detection Trigger:** "We've already invested..." as justification
**Mitigation:** Ask "Would I choose this approach if starting fresh today?"

---

### 3. **Availability Heuristic**
Judgments influenced by what springs most easily to mind.

**Software Engineering Context:**
- Using the most recent solution you remember instead of researching
- Overestimating bug frequency based on recent memorable incidents

**Detection Trigger:** Immediate recall of examples biasing decisions
**Mitigation:** Seek statistical data rather than relying on memory

---

### 4. **Curse of Knowledge**
Once you understand something, you presume it's obvious to everyone.

**Software Engineering Context:**
- Writing documentation that assumes too much knowledge
- Frustration when others don't understand "obvious" code

**Detection Trigger:** Feeling like something is "obvious" that others struggle with
**Mitigation:** Explain things as if to your past self before you learned it

---

### 5. **Confirmation Bias**
Favoring information that confirms existing beliefs.

**Software Engineering Context:**
- Only testing happy paths that confirm code works
- Seeking evidence that your design choice was correct while ignoring problems

**Detection Trigger:** Noticing you only collect supporting evidence
**Mitigation:** Actively seek disconfirming evidence; try to break your own code

---

### 6. **Dunning-Kruger Effect**
Knowing little makes you overconfident; expertise reveals uncertainty.

**Software Engineering Context:**
- New to a codebase and assuming you understand it all
- Junior developers more confident in estimates than seniors

**Detection Trigger:** High confidence despite limited experience
**Mitigation:** Assume there's more you don't know; ask "What am I missing?"

---

### 7. **Belief Bias**
If a conclusion supports existing beliefs, you'll rationalize anything supporting it.

**Software Engineering Context:**
- Defending your preferred technology despite evidence against it
- Rationalizing why your coding style is superior

**Detection Trigger:** Finding reasons to support pre-existing preferences
**Mitigation:** Ask "When and how did I develop this belief?"

---

### 8. **Self-Serving Bias**
Failures are external; successes are personal.

**Software Engineering Context:**
- "The bug was caused by bad requirements" vs "I wrote great code"
- Taking credit for team successes, blaming others for failures

**Detection Trigger:** Asymmetric attribution of credit/blame
**Mitigation:** Consider external factors in success, internal factors in failure

---

### 9. **Backfire Effect**
Challenges to core beliefs make you believe even more strongly.

**Software Engineering Context:**
- Bug reports make you defend your code more vigorously
- Evidence against your approach strengthens your commitment to it

**Detection Trigger:** Feeling attacked when your work is criticized
**Mitigation:** Separate your identity from your code; "my code has a bug" ≠ "I am a bad programmer"

---

### 10. **Barnum Effect** (Forer Effect)
Seeing personal specifics in vague statements.

**Software Engineering Context:**
- Generic tech advice feels personally applicable
- Vague descriptions of problems seeming to match your situation

**Detection Trigger:** Vague statements feeling specifically relevant
**Mitigation:** Check if the statement could apply to anyone

---

### 11. **Groupthink**
Social dynamics override best outcomes.

**Software Engineering Context:**
- Team agrees with the loudest voice in the room
- Not voicing concerns to maintain harmony

**Detection Trigger:** Quick consensus without dissent
**Mitigation:** Explicitly invite and protect dissenting opinions

---

### 12. **Negativity Bias**
Negative things disproportionately influence thinking.

**Software Engineering Context:**
- One bad code review comment overshadows ten positive ones
- Remembering failures more than successes

**Detection Trigger:** Dwelling on negatives while dismissing positives
**Mitigation:** Deliberately acknowledge positive evidence

---

### 13. **Declinism**
Remembering the past as better, expecting the future to be worse.

**Software Engineering Context:**
- "Code used to be better written"
- "New frameworks are making things worse"

**Detection Trigger:** Nostalgia for "the old days" without evidence
**Mitigation:** Use objective metrics to compare past and present

---

### 14. **Framing Effect**
Being influenced by context and delivery more than content.

**Software Engineering Context:**
- Same idea rejected when from junior, accepted from senior
- Solutions presented as "innovative" vs "risky" affecting acceptance

**Detection Trigger:** Different reactions to the same idea based on presentation
**Mitigation:** Try to extract the core content from the framing

---

### 15. **Fundamental Attribution Error**
Judging others by character, yourself by situation.

**Software Engineering Context:**
- "They wrote bad code because they're careless" vs "I wrote bad code because I was rushed"
- Assuming others' mistakes are due to incompetence

**Detection Trigger:** Different explanations for the same behavior
**Mitigation:** Assume others face the same situational pressures you do

---

### 16. **Halo Effect**
How much you like someone influences other judgments of them.

**Software Engineering Context:**
- Code from developers you like seems better
- All work from a star developer is assumed to be good

**Detection Trigger:** Consistently rating everything from one person highly
**Mitigation:** Review work blind to author when possible

---

### 17. **Optimism Bias**
Overestimating likelihood of positive outcomes.

**Software Engineering Context:**
- "This refactoring will definitely only take 2 days"
- Underestimating project complexity and risks

**Detection Trigger:** Consistently optimistic estimates that prove wrong
**Mitigation:** Track actual vs. estimated time; use historical data

---

### Additional Biases (from the 24)
*Note: The PDF extraction was partial. The complete set includes 24 biases. Other known biases in the set likely include:*

18. **Spotlight Effect** - Overestimating how much others notice your mistakes
19. **Projection Bias** - Assuming others share your beliefs and values
20. **Recency Bias** - Weighting recent information too heavily
21. **Status Quo Bias** - Preferring things to stay the same
22. **Just-World Hypothesis** - Believing good things happen to good people
23. **Hindsight Bias** - "I knew it all along" after the fact
24. **Blind Spot Bias** - Recognizing biases in others but not yourself

---

## Creative Thinking Cards - 52 Brainstorming Tools

The Creative Thinking deck includes 52 cards organized into color-coded sections:

### **Sections:**
1. **Idea Generation** - Tools for generating new possibilities
2. **Perspective Shifts** - Ways to view problems differently
3. **Limitation** - Using constraints creatively
4. **Mental Models** - Frameworks for understanding
5. **Provocation** - Challenging assumptions
6. **Group Brainstorming Tools** - Collaborative techniques

**Software Engineering Applications:**
- Sprint planning and design sessions
- Problem-solving stuck technical issues
- Architecture discussions
- Innovation workshops
- Breaking out of conventional solutions

---

## Connecting to Mid-Information Framework

Based on your earlier conversation about misinformation/disinformation/mid-information:

### **Mid-Information in Software Engineering**

**Mid-Information Zone:** The state where collective understanding is incomplete or evolving
- New frameworks/technologies where best practices aren't established
- Complex codebases where no one has complete understanding
- Emerging architectural patterns
- Performance characteristics not yet fully understood

### **Detection Signals You're in Mid-Information:**

1. **High Confidence + Low Verification**
   - You "know" how something works but haven't tested it
   - Dunning-Kruger alert: New to area but very confident

2. **Inconsistent Information**
   - Different sources contradict each other
   - Documentation doesn't match code behavior
   - Team members have different mental models

3. **Pattern Matching Fails**
   - Solutions that worked before don't work now
   - Analogies to previous problems break down
   - Texas Sharpshooter: Cherry-picking evidence that fits

4. **Defensive Reactions**
   - Backfire effect when code/approach is questioned
   - Special pleading for why tests aren't needed
   - Tu quoque responses to code reviews

5. **Assumed Knowledge**
   - Curse of knowledge making things seem "obvious"
   - Documentation gaps assumed not to matter
   - Burden of proof shifted to others

### **Moving from Mid-Information to Knowledge:**

1. **Acknowledge Uncertainty**
   - "I think X, but I could be wrong"
   - Build tests to validate assumptions
   - Document what you don't know

2. **Seek Disconfirming Evidence**
   - Try to break your own code (counter confirmation bias)
   - Look for edge cases (counter availability heuristic)
   - Ask "What would prove me wrong?"

3. **Question First Impressions**
   - Counter anchoring by considering multiple approaches
   - Avoid commitment based on sunk costs
   - Check if bandwagon/authority are your main reasons

4. **Separate Identity from Ideas**
   - Counter backfire effect by treating code as separate from self
   - Be willing to abandon approaches that aren't working
   - Accept that being wrong is how you learn

---

## Practical Applications for AI Agents

### **System Prompt Additions to Detect Mid-Information:**

```
Before providing a solution, check for these signals:

1. CONFIDENCE CHECK
   - Am I highly confident despite limited information? (Dunning-Kruger)
   - Have I verified this or am I pattern-matching? (Availability heuristic)

2. EVIDENCE CHECK
   - Am I only looking at confirming evidence? (Confirmation bias)
   - Am I cherry-picking successful examples? (Texas sharpshooter)
   - Is this anecdotal or statistical?

3. REASONING CHECK
   - Am I arguing from authority/popularity instead of merit? (Appeal to authority/Bandwagon)
   - Am I presenting false dilemmas when more options exist? (Black-or-white)
   - Is my conclusion circular? (Begging the question)

4. BIAS CHECK
   - Is the first solution I thought of anchoring me? (Anchoring)
   - Am I continuing because of sunk effort? (Sunk cost fallacy)
   - Am I defending this because it aligns with my preferences? (Belief bias)

WHEN IN DOUBT:
- State uncertainty explicitly
- Provide multiple approaches with tradeoffs
- Identify assumptions that could be tested
- Suggest verification steps
```

### **Human Collaboration Improvements:**

**For Humans to Ask Themselves:**
- "What would change my mind about this?"
- "Am I defending this because I wrote it?" (Backfire effect)
- "Would I choose this if starting fresh?" (Sunk cost fallacy)
- "Is this obvious or am I cursed with knowledge?"

**For Humans to Ask AI:**
- "What assumptions are you making?"
- "What could prove this wrong?"
- "What am I not considering?"
- "Rate your confidence and explain why"

**For AI to Ask Humans:**
- "I notice high confidence here - what's your verification?"
- "This seems like [specific bias] - can we check?"
- "Are there alternative approaches we haven't considered?"
- "Is this truly a binary choice or are there more options?"

---

## Immediate Action Items

### **For Personal Development:**

1. **Print/Download the Posters**
   - Keep visible as reference during code reviews
   - Review before making technical decisions

2. **Learn One Per Week**
   - Deep dive into one fallacy/bias each week
   - Find examples in your own recent work
   - Journal when you catch yourself in one

3. **Pre-Mortem Checklist**
   - Before committing to an approach, check the top 10 biases
   - Ask "Which biases might be affecting this decision?"

### **For Team Development:**

1. **Code Review Prompts**
   - "Am I showing confirmation bias by only looking for what I expect?"
   - "Am I being influenced by who wrote this?" (Halo effect)
   - "Is this ad hominem or technical feedback?"

2. **Estimation Sessions**
   - Counter anchoring by having people estimate independently first
   - Counter optimism bias by reviewing historical accuracy
   - Check for sunk cost fallacy in continuation decisions

3. **Architecture Discussions**
   - Explicitly invite dissent (counter groupthink)
   - Check for appeal to authority/bandwagon
   - Identify false dilemmas and seek alternatives

### **For AI Agent Development:**

1. **Add Detection Heuristics**
   - Flag high confidence + low verification
   - Detect pattern matching without verification
   - Identify when only confirming evidence is cited

2. **Explicit Uncertainty Communication**
   - "I'm in a mid-information zone here"
   - "This assumption needs verification"
   - "Multiple interpretations possible"

3. **Bias Self-Checks**
   - Before each response, run through top 5 relevant biases
   - Flag when reasoning matches known fallacy patterns
   - Offer alternative perspectives

---

## Resources for Download

**Free PDFs (Creative Commons BY-NC):**
- Logical Fallacies Poster (A1, A4 sizes)
- Cognitive Biases Poster (A1, A4 sizes)
- Critical Thinking Cards (printable)
- Creative Thinking Cards (info at thethinkingshop.org)

**How to Get:**
1. Visit schoolofthought.org/downloads
2. Enter email for download link
3. All resources sent to inbox

**Physical Products:**
Available at thethinkingshop.org
- Proceeds support the non-profit
- High-quality 310gsm card stock
- Casino-quality linen cards

---

## Summary: The Big Picture

**The Core Insight:**

True innovation happens in the **mid-information zone** - where nobody has complete understanding yet. The danger is:

1. **Not knowing you don't know** (Dunning-Kruger)
2. **Thinking you know when you don't** (Confirmation bias + Availability heuristic)
3. **Defending wrong beliefs** (Backfire effect + Sunk cost fallacy)

**The Solution:**

1. **Build awareness** of fallacies and biases
2. **Create detection triggers** for when you're in mid-information
3. **Develop practices** that counter common thinking errors
4. **Embrace uncertainty** as a signal to investigate, not a weakness

**For Software Engineering:**

Code is where our thoughts become concrete and testable. Every bug is often a thinking error made manifest. By understanding logical fallacies and cognitive biases, we can:

- Write better code (less confirmation bias in testing)
- Make better decisions (avoid false dilemmas and appeals to authority)
- Collaborate better (recognize and counter groupthink)
- Learn faster (reduce backfire effect, embrace being wrong)
- Innovate more (identify when in mid-information and leverage it)

**Your Actions and Thoughts Control Nothing But Influence Everything**

You can't control whether you'll have biases (you will). But you can influence:
- Your awareness of them
- Your practices to counter them  
- Your willingness to be wrong
- Your ability to update beliefs with new evidence

That's the path from mid-information to knowledge, and from good to great engineering.
