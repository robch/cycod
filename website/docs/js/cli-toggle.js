document.addEventListener('DOMContentLoaded', function () {
  // Loop through each cli-command block
  const cliCommandBlocks = document.querySelectorAll('.cli-command');
  cliCommandBlocks.forEach(function (cliCommandBlock) {
    // Try to find the next `cli-output` block
    let nextElement = cliCommandBlock.nextElementSibling;
    while (nextElement) {
      if (nextElement.classList.contains('cli-output')) {
        break; // Found the cli-output block
      }
      if (nextElement.classList.contains('highlight')) {
        return; // Reached the next code block and it's not a cli-output block
      }

      // Move to the next element since it's not a cli-output block
      nextElement = nextElement.nextElementSibling;
    }

    // We found the cli-output block
    let cliOutputBlock = nextElement;
    
    if (!cliOutputBlock) {
      return; // No output block found, nothing to toggle
    }

    // Create the toggle button
    const toggleButton = document.createElement('button');
    toggleButton.textContent = '';
    toggleButton.title = 'Toggle output';
    toggleButton.classList.add('toggle-output-button');
    toggleButton.classList.add("md-icon");

    // Insert the toggle button inside the command block's filename span
    const filenameSpan = cliCommandBlock.querySelector('.filename');
    if (filenameSpan) {
      filenameSpan.appendChild(toggleButton);
    } else {
      // If there's no filename span, create one
      const newFilenameSpan = document.createElement('span');
      newFilenameSpan.classList.add('filename');
      cliCommandBlock.insertBefore(newFilenameSpan, cliCommandBlock.firstChild);
      newFilenameSpan.appendChild(toggleButton);
    }

    // Add click event to toggle visibility
    toggleButton.addEventListener('click', function () {
      cliOutputBlock.classList.toggle('cli-output-open');
      toggleButton.classList.toggle('toggle-output-button-rotated');
    });
  });
});