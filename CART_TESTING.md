# Cart Module Manual Testing Guide

This document provides step-by-step instructions for manually testing the shopping cart functionality.

## Prerequisites

1. SQL Server running and accessible
2. Database migrations applied for all modules (Users, SellerPanel, ProductCatalog, Cart)
3. API running on configured port (default: https://localhost:7147)
4. UI running on configured port
5. At least one seller account with published products

## Setup Test Data

### 1. Create Test Accounts

#### Seller Account
- Email: seller@test.com
- Password: Test123!
- Role: Seller

#### Buyer Account  
- Email: buyer@test.com
- Password: Test123!
- Role: Buyer

### 2. Create Test Store and Products

Login as seller and:
1. Create a store (e.g., "Test Electronics Store")
2. Create at least 3-5 products in different price ranges
3. Ensure products are in "Published" status
4. Set reasonable stock quantities (e.g., 10-20 items each)

## Test Scenarios

### Scenario 1: Guest Cart - Add Items

**Objective**: Verify guest users can add items to cart

1. Open browser in incognito/private mode
2. Navigate to product catalog
3. Click "Add to Cart" on 2-3 different products
4. Navigate to cart page (/cart)
5. **Verify**:
   - All added items appear in cart
   - Items grouped by seller
   - Quantities are correct (1 each)
   - Prices displayed correctly
   - Subtotals calculated correctly
   - Total amount is accurate

### Scenario 2: Update Cart Quantities

**Objective**: Verify quantity changes work correctly

1. In cart, increase quantity of first item to 3
2. **Verify**:
   - Quantity updates immediately
   - Subtotal recalculates
   - Total amount updates
   
3. Decrease quantity back to 1
4. **Verify**: Amounts recalculate correctly

5. Try to set quantity to 0 or negative
6. **Verify**: Cannot set quantity below 1

### Scenario 3: Remove Items

**Objective**: Verify items can be removed from cart

1. Click "Remove" on one cart item
2. **Verify**:
   - Item disappears from cart
   - Total updates correctly
   - Other items remain

3. Remove all remaining items
4. **Verify**: "Your cart is empty" message appears

### Scenario 4: Multi-Seller Cart

**Objective**: Verify cart handles multiple sellers correctly

*Prerequisites*: Create products from 2 different seller accounts

1. Add products from Store A to cart
2. Add products from Store B to cart
3. Navigate to cart
4. **Verify**:
   - Items grouped by store
   - Each store shown in separate section
   - Store names displayed correctly
   - Totals calculated across all stores

### Scenario 5: Stock Validation

**Objective**: Verify stock limits are enforced

1. Find a product with stock quantity 5
2. Try to add 6 items to cart
3. **Verify**: Error message about insufficient stock

4. Add 3 items successfully
5. In cart, try to increase quantity to 6
6. **Verify**: Cannot increase beyond available stock

### Scenario 6: Price Changes

**Objective**: Verify price changes are detected

*This requires database manipulation*

1. Add item to cart
2. Note the price
3. Using database tool, update the product price
4. Refresh cart page
5. **Verify**:
   - Old price shown with strikethrough
   - New price highlighted
   - "Price Changes Detected" warning shown

### Scenario 7: Product Availability

**Objective**: Verify unavailable products are handled

1. Add product to cart
2. Using database tool, change product status to "Archived"
3. Refresh cart page
4. **Verify**:
   - "Unavailable Items" warning shown
   - Item marked as unavailable
   - Checkout button disabled

### Scenario 8: Guest Cart Migration

**Objective**: Verify guest cart merges into user cart on login

1. As guest, add 2-3 items to cart
2. Register new account OR login to existing account
3. After login, navigate to cart
4. **Verify**:
   - Guest cart items now in user cart
   - Quantities preserved
   - No duplicate items (or quantities combined if same product)

### Scenario 9: Session Persistence

**Objective**: Verify cart persists for authenticated users

1. Login as buyer
2. Add items to cart
3. Close browser completely
4. Reopen browser and login again
5. Navigate to cart
6. **Verify**: Cart items still present

### Scenario 10: Clear Cart

**Objective**: Verify clear cart functionality

1. Add multiple items to cart
2. Click "Clear Cart"
3. Confirm in dialog
4. **Verify**:
   - All items removed
   - "Your cart is empty" message shown
   - Total is $0.00

### Scenario 11: Max Items Limit

**Objective**: Verify 50 item limit enforced

*Note: This is tedious to test manually*

1. Add 50 different products to cart
2. Try to add 51st product
3. **Verify**: Error message about maximum items limit

### Scenario 12: Continue Shopping

**Objective**: Verify navigation works correctly

1. In cart, click "Continue Shopping"
2. **Verify**: Navigated back to catalog

3. Click cart icon/link in navigation
4. **Verify**: Returns to cart page

## API Testing (Optional)

Use tools like Postman or curl to test API endpoints directly.

### Get Cart
```bash
curl -X GET https://localhost:7147/api/cart \
  -H "X-Session-Id: test-session-123"
```

### Add Item
```bash
curl -X POST https://localhost:7147/api/cart/items \
  -H "Content-Type: application/json" \
  -H "X-Session-Id: test-session-123" \
  -d '{
    "productId": "YOUR-PRODUCT-GUID",
    "quantity": 2
  }'
```

### Update Quantity
```bash
curl -X PUT https://localhost:7147/api/cart/items/YOUR-ITEM-GUID \
  -H "Content-Type: application/json" \
  -H "X-Session-Id: test-session-123" \
  -d '{
    "quantity": 5
  }'
```

### Remove Item
```bash
curl -X DELETE https://localhost:7147/api/cart/items/YOUR-ITEM-GUID \
  -H "X-Session-Id: test-session-123"
```

### Clear Cart
```bash
curl -X DELETE https://localhost:7147/api/cart \
  -H "X-Session-Id: test-session-123"
```

## Known Limitations / TODOs

- Session ID management uses temporary GUID generation (should use localStorage in production)
- No toast notifications for cart actions (users don't see immediate feedback)
- Cart doesn't update in real-time when items are added from catalog (no auto-refresh)
- Checkout flow not yet implemented
- No "Save for Later" functionality
- No stock locking during checkout (items could be purchased by others)

## Reporting Issues

When reporting cart-related issues, please include:
- User type (guest or authenticated)
- Session ID (if guest)
- Steps to reproduce
- Expected vs actual behavior
- Browser console errors (if any)
- Network requests/responses (from browser dev tools)
