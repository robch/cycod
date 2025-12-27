# cycodj cleanup Command - Filtering Pipeline Catalog

[← Back to cycodj Catalog](cycodj-filtering-pipeline-catalog-README.md)

## Overview

The `cleanup` command finds and optionally removes duplicate, empty, or old conversations.

## Layer Documentation

Layer 7 (Output Persistence) and Layer 9 (Actions on Results) are currently documented:

- Layer 1: Target Selection - *Not yet documented*
- Layer 2: Container Filter - *Not yet documented*
- Layer 3: Content Filter - *Not yet documented*
- Layer 4: Content Removal - *Not yet documented*
- Layer 5: Context Expansion - *Not yet documented*
- Layer 6: Display Control - *Not yet documented*
- **[Layer 7: Output Persistence](cycodj-cleanup-filtering-pipeline-catalog-layer-7.md)** ✅ ([proof](cycodj-cleanup-filtering-pipeline-catalog-layer-7-proof.md)) - **NOT IMPLEMENTED**
- Layer 8: AI Processing - *Not yet documented*
- **[Layer 9: Actions on Results](cycodj-cleanup-filtering-pipeline-catalog-layer-9.md)** ✅ ([proof](cycodj-cleanup-filtering-pipeline-catalog-layer-9-proof.md)) - **IMPLEMENTED**

**Note**: The cleanup command is unique in that it implements Layer 9 (file deletion actions) but does NOT implement Layer 7 (output persistence) because it's an interactive action command requiring console feedback.
