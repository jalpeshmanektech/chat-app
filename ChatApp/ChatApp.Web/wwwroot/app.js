// Chat App JavaScript Functions


// Auto-resize textarea
window.autoResizeTextarea = function (element) {
    element.style.height = 'auto';
    element.style.height = element.scrollHeight + 'px';
};

// Scroll to bottom of messages container
window.scrollToBottom = function (element) {
    element.scrollTop = element.scrollHeight;
};

// Format timestamp
window.formatTime = function (timestamp) {
    const date = new Date(timestamp);
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
};

// Format date
window.formatDate = function (timestamp) {
    const date = new Date(timestamp);
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    if (date.toDateString() === today.toDateString()) {
        return 'Today';
    } else if (date.toDateString() === yesterday.toDateString()) {
        return 'Yesterday';
    } else {
        return date.toLocaleDateString();
    }
};

// Show notification
window.showNotification = function (title, message) {
    if ('Notification' in window && Notification.permission === 'granted') {
        new Notification(title, { body: message });
    }
};

// Request notification permission
window.requestNotificationPermission = function () {
    if ('Notification' in window && Notification.permission === 'default') {
        Notification.requestPermission();
    }
};

// Copy text to clipboard
window.copyToClipboard = function (text) {
    navigator.clipboard.writeText(text).then(function () {
        console.log('Text copied to clipboard');
    }).catch(function (err) {
        console.error('Failed to copy text: ', err);
    });
};

// Download file
window.downloadFile = function (url, filename) {
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};

// Play notification sound
window.playNotificationSound = function () {
    const audio = new Audio('/sounds/notification.mp3');
    audio.play().catch(function (error) {
        console.log('Could not play notification sound:', error);
    });
};

// Debounce function
window.debounce = function (func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
};

// Throttle function
window.throttle = function (func, limit) {
    let inThrottle;
    return function () {
        const args = arguments;
        const context = this;
        if (!inThrottle) {
            func.apply(context, args);
            inThrottle = true;
            setTimeout(() => inThrottle = false, limit);
        }
    };
};

// Check if element is in viewport
window.isInViewport = function (element) {
    const rect = element.getBoundingClientRect();
    return (
        rect.top >= 0 &&
        rect.left >= 0 &&
        rect.bottom <= (window.innerHeight || document.documentElement.clientHeight) &&
        rect.right <= (window.innerWidth || document.documentElement.clientWidth)
    );
};

// Smooth scroll to element
window.smoothScrollTo = function (element) {
    element.scrollIntoView({
        behavior: 'smooth',
        block: 'nearest',
        inline: 'nearest'
    });
};

// Get file size in human readable format
window.formatFileSize = function (bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};

// Check if file is an image
window.isImageFile = function (filename) {
    const imageExtensions = ['.jpg', '.jpeg', '.png', '.gif', '.bmp', '.webp'];
    const extension = filename.toLowerCase().substring(filename.lastIndexOf('.'));
    return imageExtensions.includes(extension);
};

// Check if file is a video
window.isVideoFile = function (filename) {
    const videoExtensions = ['.mp4', '.avi', '.mov', '.wmv', '.flv', '.webm'];
    const extension = filename.toLowerCase().substring(filename.lastIndexOf('.'));
    return videoExtensions.includes(extension);
};

// Check if file is a document
window.isDocumentFile = function (filename) {
    const documentExtensions = ['.pdf', '.doc', '.docx', '.txt', '.rtf'];
    const extension = filename.toLowerCase().substring(filename.lastIndexOf('.'));
    return documentExtensions.includes(extension);
};

// Add this for file input trigger
window.triggerFileInput = function (element) {
    element.click();
}; 

window.emojiInterop = {
     showPicker: function (dotNetRef) {
          const container = document.getElementById('emoji-picker-container');
          if (!container.firstChild) {
               const picker = document.createElement('emoji-picker');
               picker.addEventListener('emoji-click', event => {
                    dotNetRef.invokeMethodAsync('ReceiveEmoji', event.detail.unicode);
                    container.style.display = 'none';
               });
               container.appendChild(picker);
          }

          container.style.display = container.style.display === 'none' ? 'block' : 'none';
     }
};