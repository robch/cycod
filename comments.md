In doc:
> exceed the capabilities of Claude Code 

Luke's comment:
> There's a few strong claims here - which I'll be curious to learn more about as I read the doc :-).
>
> But I think what might be helpful up front is a little bit more distinction on why we can be fundametnally differentiated here?  What allows us to uniquely differentiate from Claude Code medium-to-long term?  Why/when is a CLI better than IDE integration?  Etc.

---

In doc:
> surpass what's available in today's IDEs

Luke's comment:
> Why not add it to our IDEs then (where millions of developers are already engaged)?

---

In doc:
> allowing developers to be the "superheroes" who decide exactly how AI augments their workflow.

Luke's comment:
> How so?  Why is a CLI needed to deliver on this?

---

In doc:
> terminal-based AI coding assistant market 

Luke's comment:
> Are we convinced this is a separate "market"?  

---

In doc:
> The need for powerful tools to systematically explore and curate context that far exceeds current IDE capabilities.

Luke's comment:
> This feels orthogonal to delivering a CLI.  It feels like something we want in Copilot generally in the IDE, in GitHub, *and* in the CLI?
> 
> Is there a reason this is uniquely important or possible in a CLI? 
> 
> Is there a reason to *first* tackle it in a CLI (where we don't yet have existing engaged users) vs. as a feature in GitHub or VS Code?

---

In doc:
> Addressing expensive per-token pricing scenarios.

Luke's comment:
> I don't follow what need exists here?  That good coding models are expensive?

---

In doc:
> 4.	Multi-Model Flexibility: Flexible selection of the AI model based on the task at hand.

Matt's comment:
> I was under the impression (could have changed) that the plan for Padawan was "don't worry about the model, that's an implementation detail and we'll just make sure that it uses "the best one".  This feels like it's the exact opposite of our plan for Padawan.  Do we need to harmonize the two apporaches, or are we shifting our thinking for Padawan, or do we feel like it's okay that they diverge in this case?

---

In doc:
> 5.	Variable Autonomy

Luke's comment:
> What is missing from Claude Code here?

---

In doc:
> 6.	Deep GitHub Integration: Seamless integration with GitHub workflows, from issues and PRs to branch management.

Matt's comment:
> Does this mean there is stuff that is missing from the existing `gh` CLI - or is this more about ensuring the existing tools surfaced by `gh` are exposed in a terminal based AI coding tool/assistant?

---

In doc:
> 1.	The Context Slider

Luke's comment:
> I'd love more details on this.  
> 
> And if it is so revolutionary - let's get it into VS Code!

---

In doc:
> PowerShell workflows.

Matt's comment:
> My assumption is that this means the tool "plays nicely" with the object pipeline in PowerShell, but is there something different here?  Are there analogs for "bash workflows" and we just don't care about them, or something else?

---

In doc:
> Key Differentiating Capabilities

Luke's comment:
> I feel like the same thing, at the same high level of abstraction, is repeated a lot in this doc :-).
> 
> This is the 4th of 5 places where "context exploration" is listed as a 1-2 sentence bullet high level note.

---

In doc:
> github-copilot 

Matt's comment:
> Maybe this is getting in to too much detail too early, but this makes me think that `github-copilot` is a net new CLI, completely seperate from `gh`.  I would have sort of imagined that this would look like `gh copilot` instead of `github-copilot`.  I am not sure if that decision is something we need to concern ourselves with early, but I think it might put constraints on how we build this CLI (since I think then the path of least resistance would be to use `go` like other gh extensions?)

