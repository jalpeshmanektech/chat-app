# CSS Files for Chat App

This directory contains all the CSS files for the Chat App, converted from SCSS to regular CSS for easier use.

## File Structure

### 1. `app.css`
- **Purpose**: Main application styles and legacy Blazor styles
- **Contains**: 
  - CSS variables (custom properties)
  - Reset and base styles
  - Global styles
  - Legacy Blazor component styles
- **Usage**: Include this file first in your HTML

### 2. `chat-app.css`
- **Purpose**: Main chat application styles
- **Contains**:
  - Chat app layout styles
  - Chat list styles
  - Message styles
  - User sidebar styles
  - Responsive design
- **Usage**: Include after `app.css`

### 3. `components.css`
- **Purpose**: Component-specific styles
- **Contains**:
  - ChatList component styles
  - ChatListItem component styles
  - ChatWindow component styles
  - MessageInput component styles
  - UserSidebar component styles
  - Message component styles
  - Loading states
  - Error states
  - Empty states
- **Usage**: Include after `chat-app.css`

### 4. `layout.css`
- **Purpose**: Layout and navigation styles
- **Contains**:
  - Main layout styles
  - Navigation menu styles
  - Sidebar styles
  - Header and footer styles
  - Grid system
  - Layout utility classes
- **Usage**: Include after `components.css`

### 5. `utility-classes.css`
- **Purpose**: Utility classes for common styling needs
- **Contains**:
  - Spacing utilities (margin, padding)
  - Text utilities (alignment, weight, decoration)
  - Color utilities
  - Border utilities
  - Display utilities
  - Flex utilities
  - Position utilities
  - Size utilities
  - Responsive utilities
- **Usage**: Include last for utility classes to take precedence

## How to Include CSS Files

Add these CSS files to your HTML in the following order:

```html
<link href="app.css" rel="stylesheet" />
<link href="chat-app.css" rel="stylesheet" />
<link href="components.css" rel="stylesheet" />
<link href="layout.css" rel="stylesheet" />
<link href="utility-classes.css" rel="stylesheet" />
```

Or in your Blazor component:

```razor
@page "/"
@using Microsoft.AspNetCore.Components.Web

<PageTitle>Chat App</PageTitle>

<HeadContent>
    <link href="app.css" rel="stylesheet" />
    <link href="chat-app.css" rel="stylesheet" />
    <link href="components.css" rel="stylesheet" />
    <link href="layout.css" rel="stylesheet" />
    <link href="utility-classes.css" rel="stylesheet" />
</HeadContent>
```

## CSS Variables (Custom Properties)

The app uses CSS custom properties for consistent theming:

```css
:root {
  --primary-color: #ff0000;
  --background-dark: #1a1a1a;
  --text-color: #ffffff;
  --text-secondary: #999999;
  --border-color: #333333;
  --input-background: #2a2a2a;
  --hover-color: #2d2d2d;
  --message-bubble: #2a2a2a;
  --online-status: #00ff00;
  --error-color: #ff3333;
  --chat-background: #1a1a1a;
  --sidebar-background: #1f1f1f;
  --message-time: #666666;
  --attachment-background: #2f2f2f;
  --search-background: #252525;
}
```

## Key Features

### 1. Dark Theme
- All components are styled with a dark theme
- Uses CSS variables for easy theme switching

### 2. Responsive Design
- Mobile-first approach
- Responsive breakpoints for different screen sizes
- Mobile navigation with slide-out menus

### 3. Custom Scrollbars
- Styled scrollbars for better visual consistency
- WebKit scrollbar styling included

### 4. Utility Classes
- Comprehensive utility class system
- Similar to Bootstrap utilities
- Easy to use for quick styling

### 5. Component-Specific Styles
- Each component has its own CSS classes
- Modular approach for easy maintenance

## Usage Examples

### Basic Chat Layout
```html
<div class="chat-app">
  <div class="chat-app__main">
    <div class="chat-list">
      <!-- Chat list content -->
    </div>
    <div class="chat-app__content">
      <div class="chat-window">
        <!-- Chat window content -->
      </div>
    </div>
  </div>
  <div class="chat-app__sidebar">
    <!-- User sidebar content -->
  </div>
</div>
```

### Using Utility Classes
```html
<div class="d-flex justify-content-between align-items-center p-3">
  <h1 class="text-primary font-weight-bold">Chat App</h1>
  <button class="btn btn-primary rounded">Send</button>
</div>
```

### Responsive Design
```html
<div class="d-none d-md-block">
  <!-- Only visible on medium screens and up -->
</div>
<div class="d-block d-md-none">
  <!-- Only visible on small screens -->
</div>
```

## Browser Support

- Modern browsers with CSS Grid and Flexbox support
- CSS custom properties (variables)
- WebKit scrollbar styling
- CSS animations and transitions

## Customization

To customize the theme, modify the CSS variables in `app.css`:

```css
:root {
  --primary-color: #your-color;
  --background-dark: #your-background;
  /* ... other variables */
}
```

## Notes

- All SCSS has been converted to regular CSS
- No build process required for CSS compilation
- Files are organized by functionality
- Utility classes provide quick styling options
- Responsive design included for mobile devices 