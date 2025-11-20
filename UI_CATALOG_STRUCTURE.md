# Catalog Page UI Structure

## Layout Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Product Catalog                               │
│                Browse products from all sellers                   │
├─────────────────────────────────────────────────────────────────┤
│ ┌───────────────────────────────────────────────────────────┐   │
│ │  SEARCH & FILTERS PANEL                                   │   │
│ │                                                            │   │
│ │  ┌─────────────────────────────────┐  ┌────────────────┐ │   │
│ │  │ 🔍 Search products...           │  │ Sort by:       │ │   │
│ │  │                                  │  │ ▼ Newest First│ │   │
│ │  └─────────────────────────────────┘  └────────────────┘ │   │
│ │  [Search]  [Clear]                                        │   │
│ │                                                            │   │
│ │  Category        Min Price    Max Price    [Filter]       │   │
│ │  ▼ All           $______      $______                     │   │
│ │                                                            │   │
│ │  Seller                                                   │   │
│ │  ▼ All Sellers                                            │   │
│ └───────────────────────────────────────────────────────────┘   │
│                                                                  │
│  Showing 1 - 12 of 45 products                                  │
│                                                                  │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐                │
│ │ [Image] │ │ [Image] │ │ [Image] │ │ [Image] │                │
│ │         │ │         │ │         │ │         │                │
│ │ Title   │ │ Title   │ │ Title   │ │ Title   │                │
│ │ 🏪 Store│ │ 🏪 Store│ │ 🏪 Store│ │ 🏪 Store│                │
│ │ 🏷️ Cat  │ │ 🏷️ Cat  │ │ 🏷️ Cat  │ │ 🏷️ Cat  │                │
│ │ Desc... │ │ Desc... │ │ Desc... │ │ Desc... │                │
│ │ $99.99  │ │ $149.99 │ │ $79.99  │ │ $199.99 │                │
│ │ Stock:5 │ │ Stock:10│ │ Stock:2 │ │ Stock:15│                │
│ │ ★★★★☆(0)│ │ ★★★★☆(0)│ │ ★★★★☆(0)│ │ ★★★★☆(0)│                │
│ │[View]   │ │[View]   │ │[View]   │ │[View]   │                │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘                │
│                                                                  │
│ ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐                │
│ │ [Image] │ │ [Image] │ │ [Image] │ │ [Image] │                │
│ │   ...   │ │   ...   │ │   ...   │ │   ...   │                │
│ └─────────┘ └─────────┘ └─────────┘ └─────────┘                │
│                                                                  │
│          [<<] [<] [1] [2] [3] [4] [5] [>] [>>]                  │
└─────────────────────────────────────────────────────────────────┘
```

## Component Breakdown

### 1. Search Bar Section
- **Full-width search input** with search icon
- **Search button** to trigger search
- **Clear button** (conditional - only shows when filters are active)
- Real-time input binding

### 2. Filter Row 1
- **Category dropdown**: All active categories from database
- **Min Price input**: Numeric input for minimum price
- **Max Price input**: Numeric input for maximum price  
- **Filter button**: Applies price range filters

### 3. Filter Row 2
- **Seller dropdown**: All active stores from database
- Auto-applies when selection changes

### 4. Sort Dropdown
- Price: Low to High
- Price: High to Low
- Title: A-Z
- Title: Z-A
- Newest First (default)
- Oldest First

### 5. Results Summary
- Shows "X - Y of Z products"
- Updates dynamically based on filters

### 6. Product Grid
- **Responsive layout**: 
  - Large screens (lg): 4 columns
  - Medium screens (md): 3 columns
  - Small screens: 1 column
- Each card contains:
  - Product image (200px height, cover fit) or placeholder
  - Product title
  - **Seller name** with shop icon (NEW)
  - Category name with tag icon
  - Description (truncated to 3rem height)
  - Price in USD
  - Stock quantity
  - **Rating placeholder** (5 stars, hardcoded for now)
  - View Details button

### 7. Pagination
- First page button (<<)
- Previous page button (<)
- Page numbers (shows 5 at a time)
- Next page button (>)
- Last page button (>>)
- Disabled state for unavailable navigation
- Current page highlighted

## Interactive Features

### Filter Behavior
1. **Search**: Type in search box → Click Search button
2. **Category**: Select from dropdown → Auto-applies
3. **Price**: Enter min/max → Click Filter button
4. **Seller**: Select from dropdown → Auto-applies
5. **Sort**: Select option → Auto-applies
6. **Clear**: Resets all filters and returns to page 1

### Pagination Behavior
- Clicking page number navigates to that page
- Maintains current filters and search
- Shows loading spinner while fetching
- Updates results summary

### Loading States
- Shows spinner while loading products
- Shows "No products found" when no results match filters

## CSS Classes Used

### Bootstrap 5 Classes
- `container`, `row`, `col-*`
- `card`, `card-body`, `card-title`, `card-text`, `card-img-top`
- `form-control`, `form-select`, `form-label`
- `input-group`, `input-group-text`
- `btn`, `btn-primary`, `btn-outline-secondary`, `btn-sm`
- `pagination`, `page-item`, `page-link`
- `alert`, `alert-info`
- `spinner-border`
- `text-muted`, `text-primary`, `text-warning`
- `mt-*`, `mb-*`, `d-flex`, `justify-content-*`, `align-items-*`

### Bootstrap Icons
- `bi-search` - Search icon
- `bi-x-circle` - Clear filters icon
- `bi-funnel` - Filter icon
- `bi-shop` - Store/seller icon
- `bi-tag` - Category icon
- `bi-image` - Placeholder image icon
- `bi-eye` - View details icon
- `bi-star`, `bi-star-fill` - Rating stars
- `bi-chevron-*` - Pagination arrows
- `bi-info-circle` - Info icon

## Responsive Breakpoints

```css
/* Small devices (phones, < 768px) */
col-12 → 1 column grid

/* Medium devices (tablets, >= 768px) */
col-md-4, col-md-3 → 3 column grid for products

/* Large devices (desktops, >= 992px) */
col-lg-3 → 4 column grid for products
```

## Key UX Improvements

1. **Visual Hierarchy**: Search and filters prominently placed at top
2. **Progressive Disclosure**: Filters in collapsible panel
3. **Immediate Feedback**: Loading spinners, disabled states
4. **Clear Actions**: Distinct buttons for Search, Filter, Clear
5. **Information Density**: Balanced product cards with key info
6. **Accessibility**: Proper labels, ARIA attributes, keyboard navigation
7. **Mobile-First**: Responsive grid adapts to screen size
8. **Performance**: Pagination prevents loading thousands of products

## Future UI Enhancements

1. **Filter Chips**: Show active filters as removable chips
2. **Infinite Scroll**: Alternative to pagination
3. **Grid/List Toggle**: Switch between card and list view
4. **Quick View**: Modal preview without leaving catalog
5. **Saved Searches**: Save filter combinations
6. **Compare Products**: Select multiple for side-by-side comparison
7. **Advanced Filters**: Expandable panel with more options
8. **Search Autocomplete**: Suggest products as you type
