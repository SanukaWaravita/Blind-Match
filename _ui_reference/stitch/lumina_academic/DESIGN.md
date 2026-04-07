# Design System Specification: The Academic Atelier

## 1. Overview & Creative North Star
**Creative North Star: "The Fluid Scholar"**
This design system moves away from the rigid, sterile "dashboard" look typical of educational software. Instead, it adopts an editorial, high-end atmosphere that feels like a collaborative workspace for modern innovators. We achieve this through **Organic Professionalism**: a blend of high-utility "Linear-style" layouts with the soft, ethereal depth found in premium fintech apps like Stripe.

To break the "template" look, we use intentional asymmetry and **Tonal Depth**. Instead of boxing information into a grid of squares, we use varied surface heights and ample breathing room to guide the eye. This isn't just a tool; it’s a canvas for academic progress.

---

## 2. Colors & Surface Architecture

### Palette Strategy
The color system is rooted in the `primary` (Deep Academic Blue) and `secondary` (Visionary Purple), balanced by a calming `tertiary` (Teal).

*   **Primary (`#005ac8`):** Use for high-priority actions and brand-defining moments.
*   **Secondary Gradient (`#7B61FF` to `#9F7AEA`):** Reserved for "Progress" states, active project milestones, and high-level summaries.
*   **Tertiary (`#006b60`):** Used for collaborative elements, peer reviews, and "Success" indicators.

### The "No-Line" Rule
**Borders are prohibited for sectioning.** We do not use 1px lines to separate the sidebar from the main content or to divide cards. Boundaries are defined exclusively through:
1.  **Background Shifts:** Placing a `surface_container_low` section against a `surface` background.
2.  **Tonal Transitions:** Using subtle shifts in color temperature to indicate a change in context.

### Surface Hierarchy & Nesting
Treat the UI as a series of stacked sheets of fine, semi-translucent paper.
*   **Level 0 (Base):** `surface` (`#f7f9fb`) — The foundation.
*   **Level 1 (Sectioning):** `surface_container_low` — Use for large background areas like the main project feed.
*   **Level 2 (Interaction):** `surface_container_lowest` (`#ffffff`) — Use for cards and interactive modules to make them "pop" against the base.

### The "Glass & Gradient" Rule
For floating elements (modals, popovers, navigation highlights), use **Glassmorphism**. Apply a `surface_container_lowest` color at 80% opacity with a `20px` backdrop-blur. Main CTAs should utilize a subtle vertical gradient from `primary` to `primary_dim` to add "soul" and dimension.

---

## 3. Typography
We employ a dual-font strategy to balance authority with modern readability.

*   **The Editorial Voice (Display & Headline):** **Manrope.** Its geometric yet warm curves provide a premium, custom feel. Use `display-lg` (3.5rem) for project titles and `headline-md` for section headers.
*   **The Functional Voice (Title, Body, Labels):** **Inter.** The gold standard for UI legibility. Use `body-md` (0.875rem) for most content to maximize white space.

**Hierarchy Note:** Use `on_surface_variant` (muted grey) for metadata to ensure the `on_surface` (deep charcoal) headlines remain the focal point.

---

## 4. Elevation & Depth

### The Layering Principle
Depth is achieved by "stacking" tones. 
*   **Example:** A `surface_container_lowest` card sitting on a `surface_container_low` background creates a natural lift. This "soft-depth" replaces the need for heavy shadows.

### Ambient Shadows
Where floating effects are required (e.g., active task cards):
*   **Shadow:** `0px 12px 32px rgba(0, 90, 200, 0.06)`
*   **Logic:** Shadows must be ultra-diffused and tinted with the `primary` or `on_surface` color. Never use pure black shadows.

### The "Ghost Border" Fallback
If accessibility requires a container boundary, use a **Ghost Border**:
*   `outline_variant` at **15% opacity**. It should be felt, not seen.

---

## 5. Components

### Buttons
*   **Primary:** High-gloss gradient (`#4F8CFF` to `#005ac8`). Rounded `md` (0.75rem). No shadow, except on hover (Ambient Shadow).
*   **Secondary:** `surface_container_highest` with `primary` text.
*   **Tertiary:** Ghost style. No background, `primary` text, shifts to `surface_container_low` on hover.

### Cards & Lists
*   **Rule:** Forbid divider lines. Use `1.5rem` (xl) vertical spacing between list items.
*   **Style:** `surface_container_lowest` background with a `DEFAULT` (0.5rem) or `md` (0.75rem) corner radius.

### Input Fields
*   **Style:** Minimalist. No bottom line. Use `surface_container_low` as the background.
*   **Focus State:** A 2px `primary` ghost border (20% opacity) and a subtle increase in background brightness.

### Floating Progress Chips
*   Instead of standard bars, use high-contrast chips using the `secondary_container` and `tertiary_container` colors to denote project status (e.g., "In Research" or "Peer Review").

---

## 6. Do’s and Don’ts

### Do
*   **DO** use whitespace as a functional tool. If a screen feels cluttered, increase the padding, don't add a border.
*   **DO** use `surface_bright` for the top header to create a "lighthouse" effect for navigation.
*   **DO** use the `xl` (1.5rem) corner radius for large dashboard containers to soften the "tech" feel.

### Don't
*   **DON'T** use 100% black (`#000000`) for text. Use `on_surface` (`#2d3337`) to maintain the "Soft Minimal" aesthetic.
*   **DON'T** use standard 4px rounded corners. It looks dated. Stick to the `8px-16px` (md to xl) range.
*   **DON'T** use shadows on every element. Only use them for elements that "float" over others (Modals, Tooltips, Active Draggable Cards).