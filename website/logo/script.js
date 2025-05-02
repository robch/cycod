document.addEventListener('DOMContentLoaded', () => {
    // Initialize matrix background
    initMatrixBackground();
    
    // Initialize typing animation
    initTypingAnimation();
    
    // Add hover effect to logo
    document.getElementById('logo-ascii').addEventListener('mouseover', () => {
        addRandomGlitchToLogo();
    });
});

// Matrix-like falling characters animation
function initMatrixBackground() {
    const canvas = document.createElement('canvas');
    const matrix = document.getElementById('matrix-canvas');
    matrix.appendChild(canvas);
    
    const ctx = canvas.getContext('2d');
    
    // Make canvas full screen
    canvas.height = window.innerHeight;
    canvas.width = window.innerWidth;
    
    // Characters to use in the animation
    const chars = 'アァカサタナハマヤャラワガザダバパイィキシチニヒミリヰギジヂビピウゥクスツヌフムユュルグズブヅプエェケセテネヘメレヱゲゼデベペオォコソトノホモヨョロヲゴゾドボポヴッン0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    
    // Convert chars string to array
    const charArray = chars.split('');
    
    // Number of columns (based on font size)
    const fontSize = 14;
    // Double the number of columns by dividing by half the font size
    const columns = canvas.width / (fontSize / 2);
    
    // Array to track the y position of each column
    const drops = [];
    
    // Initialize drops array
    for (let i = 0; i < columns; i++) {
        drops[i] = 1;
    }
    
    // Main animation loop
    function draw() {
        // Add semi-transparent black rectangle to create fade effect
        // Increased opacity to make background less contrasty
        ctx.fillStyle = 'rgba(0, 0, 0, 0.12)';
        ctx.fillRect(0, 0, canvas.width, canvas.height);
        
        // Set text color - reducing opacity for less contrast
        ctx.fillStyle = 'rgba(0, 255, 255, 0.5)';
        ctx.font = fontSize + 'px monospace';
        
        // Loop through each drop
        for (let i = 0; i < drops.length; i++) {
            // Choose a random character
            const text = charArray[Math.floor(Math.random() * charArray.length)];
            
            // Draw the character
            ctx.fillText(text, i * fontSize, drops[i] * fontSize);
            
            // Reset drop if it's at the bottom or randomly
            // Increased probability threshold to reduce character density
            if (drops[i] * fontSize > canvas.height && Math.random() > 0.985) {
                drops[i] = 0;
            }
            
            // Move the drop down
            drops[i]++;
        }
    }
    
    // Handle window resize
    window.addEventListener('resize', () => {
        canvas.height = window.innerHeight;
        canvas.width = window.innerWidth;
        
        // Recalculate columns
        const columns = canvas.width / fontSize;
        
        // Reinitialize drops array
        for (let i = 0; i < columns; i++) {
            if (!drops[i]) drops[i] = 1;
        }
    });
    
    // Start animation - slowed down for less visual distraction
    setInterval(draw, 60);
}

// Typing animation for the tagline
function initTypingAnimation() {
    const phrases = [
        "Building the future of AI...",
        "Something incredible is coming soon...",
        "The next evolution of technology...",
        "Redefining intelligence..."
    ];
    
    const typingElement = document.getElementById('typing-text');
    let phraseIndex = 0;
    let charIndex = 0;
    let isDeleting = false;
    let typingSpeed = 100; // Base typing speed in ms
    
    function type() {
        const currentPhrase = phrases[phraseIndex];
        
        if (isDeleting) {
            // Remove a character
            typingElement.textContent = currentPhrase.substring(0, charIndex - 1);
            charIndex--;
            typingSpeed = 50; // Delete faster
        } else {
            // Add a character
            typingElement.textContent = currentPhrase.substring(0, charIndex + 1);
            charIndex++;
            typingSpeed = 100; // Type at normal speed
        }
        
        // Logic for cycling through phrases
        if (!isDeleting && charIndex === currentPhrase.length) {
            // Completed typing the phrase, pause before deleting
            isDeleting = true;
            typingSpeed = 1500; // Pause at the end of the phrase
        } else if (isDeleting && charIndex === 0) {
            // Completed deleting, move to next phrase
            isDeleting = false;
            phraseIndex = (phraseIndex + 1) % phrases.length;
            typingSpeed = 500; // Pause before typing the next phrase
        }
        
        // Schedule next character
        setTimeout(type, typingSpeed);
    }
    
    // Start the typing animation
    setTimeout(type, 1000);
}

// Add random glitch effects to logo
function addRandomGlitchToLogo() {
    const logo = document.getElementById('logo-ascii');
    
    // Already has glitch class, don't add another
    if (logo.classList.contains('glitching')) return;
    
    logo.classList.add('glitching');
    
    // Random displacement values
    const glitchAmount = 2;
    const glitches = 3 + Math.floor(Math.random() * 5); // 3 to 7 glitches
    
    // Apply multiple glitches in a sequence
    let glitchCount = 0;
    
    function applyGlitch() {
        if (glitchCount >= glitches) {
            logo.style.transform = 'translate(0, 0)';
            logo.classList.remove('glitching');
            return;
        }
        
        const xOffset = (Math.random() - 0.5) * glitchAmount * 2;
        const yOffset = (Math.random() - 0.5) * glitchAmount * 2;
        
        logo.style.transform = `translate(${xOffset}px, ${yOffset}px)`;
        
        glitchCount++;
        setTimeout(applyGlitch, 100);
    }
    
    applyGlitch();
}