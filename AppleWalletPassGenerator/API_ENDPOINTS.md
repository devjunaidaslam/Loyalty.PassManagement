# Apple Wallet Pass Generator API Endpoints

This document describes the API endpoints for managing Apple Wallet passes with push notifications and point updates.

## Database Setup

The application uses Entity Framework Core with SQL Server LocalDB. The database will be automatically created when the application starts.

## Endpoints

### 1. Generate Pass
**POST** `/Pass/generate`

Creates a new Apple Wallet pass and stores the data in the database.

**Request Body:**
```json
{
  "serialNumber": "string (optional - auto-generated if not provided)",
  "customerName": "string (required)",
  "customerEmail": "string (required)",
  "points": "number (default: 0)",
  "barcodeMessage": "string (optional - uses serialNumber if not provided)",
  "qrCodeData": "string (optional - auto-generated if not provided)"
}
```

**Response:** Returns the `.pkpass` file for download.

### 2. Update Points with Push Notification
**POST** `/Pass/update-points`

Updates the points for an existing pass and sends a push notification to the registered device.

**Request Body:**
```json
{
  "serialNumber": "string (required)",
  "points": "number (required)",
  "deviceToken": "string (optional - updates device token if provided)"
}
```

**Response:**
```json
{
  "message": "Points updated successfully",
  "serialNumber": "string",
  "points": "number",
  "pushNotificationSent": "boolean"
}
```

### 3. Get Latest Pass (Apple Wallet Web Service)
**GET** `/Pass/passes/{serialNumber}`

This endpoint is called by Apple Wallet when a push notification is received to fetch the latest pass data.

**Parameters:**
- `serialNumber`: The pass serial number

**Response:** Returns the updated `.pkpass` file.

### 4. Register Device for Push Notifications
**POST** `/Pass/passes/{serialNumber}/registrations/{deviceToken}`

Registers a device token for push notifications for a specific pass.

**Parameters:**
- `serialNumber`: The pass serial number
- `deviceToken`: The device token from Apple Push Notification service

**Response:**
```json
{
  "message": "Device registered successfully"
}
```

### 5. Unregister Device
**DELETE** `/Pass/passes/{serialNumber}/registrations/{deviceToken}`

Removes device registration for push notifications.

**Parameters:**
- `serialNumber`: The pass serial number
- `deviceToken`: The device token to remove

**Response:**
```json
{
  "message": "Device unregistered successfully"
}
```

### 6. Get Pass Log
**GET** `/Pass/passes/{serialNumber}/log`

Retrieves information about a pass including version, last update time, and current points.

**Parameters:**
- `serialNumber`: The pass serial number

**Response:**
```json
{
  "serialNumber": "string",
  "version": "number",
  "lastUpdated": "datetime",
  "points": "number",
  "hasDeviceToken": "boolean"
}
```

## Apple Wallet Integration Flow

1. **Initial Pass Creation:**
   - Call `/Pass/generate` to create a new pass
   - The pass is stored in the database with initial data

2. **Device Registration:**
   - When a user adds the pass to their Apple Wallet, Apple will call `/Pass/passes/{serialNumber}/registrations/{deviceToken}`
   - This registers the device for push notifications

3. **Updating Points:**
   - Call `/Pass/update-points` with the new points
   - The system updates the database and sends a push notification
   - Apple Wallet receives the notification and calls `/Pass/passes/{serialNumber}` to get the updated pass

4. **Pass Updates:**
   - Apple Wallet calls `/Pass/passes/{serialNumber}` to fetch the latest pass data
   - The system generates a new pass with updated information

## Push Notification Configuration

The push notification service is configured to work with Apple Push Notification service (APNs). For production use, you'll need to:

1. Configure proper APNs certificates
2. Update the push notification service to use your APNs credentials
3. Ensure your server can reach Apple's APNs servers

## Database Schema

The `PassData` table stores:
- `SerialNumber` (Primary Key): Unique identifier for the pass
- `CustomerName`: Customer's name
- `CustomerEmail`: Customer's email
- `Points`: Current loyalty points
- `BarcodeMessage`: Barcode data
- `QrCodeData`: QR code data
- `DeviceToken`: Device token for push notifications
- `CreatedAt`: Creation timestamp
- `UpdatedAt`: Last update timestamp
- `Version`: Pass version number (incremented on updates)

## Error Handling

All endpoints include proper error handling and logging. Common error responses:

- `400 Bad Request`: Missing required parameters
- `404 Not Found`: Pass not found
- `500 Internal Server Error`: Server error

## Testing

You can test the endpoints using:
1. Swagger UI (available in development mode)
2. Postman or similar API testing tools
3. The provided HTTP file (`AppleWalletPassGenerator.http`)

## Security Considerations

- Ensure proper authentication for production use
- Validate all input parameters
- Use HTTPS in production
- Secure your APNs certificates
- Implement rate limiting for push notifications
