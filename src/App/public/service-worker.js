// Handle push events for push notifications
self.addEventListener('push', (event) => {
    if (!event.data) {
        console.log('Push event received but no data');
        return;
    }

    let notificationData = {};

    try {
        notificationData = event.data.json();
    } catch (e) {
        notificationData = {
            title: 'Attendr Notification',
            body: event.data.text(),
            icon: '/images/attendr-icon.png'
        };
    }

    const options = {
        body: notificationData.body,
        icon: notificationData.icon || '/images/attendr-icon.png',
        badge: notificationData.badge || '/images/attendr-icon.png',
        tag: notificationData.tag || 'attendr-notification',
        requireInteraction: notificationData.requireInteraction || false,
        data: notificationData.data || {}
    };

    if (notificationData.actions) {
        options.actions = notificationData.actions;
    }

    event.waitUntil(
        self.registration.showNotification(
            notificationData.title || 'Attendr',
            options
        )
    );
});

// Handle notification clicks
self.addEventListener('notificationclick', (event) => {
    event.notification.close();

    // Handle action clicks
    if (event.action) {
        const action = event.notification.data?.actions?.find(
            (a) => a.action === event.action
        );
        if (action && action.url) {
            event.waitUntil(
                clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
                    // Check if a window with the target URL is already open
                    for (let client of clientList) {
                        if (client.url === action.url && 'focus' in client) {
                            return client.focus();
                        }
                    }
                    // If not, open a new window
                    if (clients.openWindow) {
                        return clients.openWindow(action.url);
                    }
                })
            );
        }
        return;
    }

    // Default: navigate to the notification's data URL or home
    const url = event.notification.data?.url || '/app/dashboard';
    event.waitUntil(
        clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
            // Check if a window with the target URL is already open
            for (let client of clientList) {
                if (client.url === url && 'focus' in client) {
                    return client.focus();
                }
            }
            // If not, open a new window
            if (clients.openWindow) {
                return clients.openWindow(url);
            }
        })
    );
});

// Handle notification close
self.addEventListener('notificationclose', (event) => {
    console.log('Notification closed:', event.notification.tag);
});
