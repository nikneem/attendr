// Handle push events for push notifications
self.addEventListener('push', (event) => {
    // Stop event propagation to prevent other service workers from handling it
    event.stopImmediatePropagation();

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
            icon: '/images/logo-192x192.png'
        };
    }

    const options = {
        body: notificationData.body,
        icon: notificationData.icon || '/images/logo-192x192.png',
        badge: notificationData.badge || '/images/logo-192x192.png',
        tag: notificationData.tag || 'attendr-notification',
        requireInteraction: notificationData.requireInteraction || false,
        data: {}
    };

    // If notification has a URL, add a "Show More" action button
    if (notificationData.url) {
        options.data.url = notificationData.url;
        options.actions = [{
            action: 'show-more',
            title: 'Show More'
        }];
    }

    // Add any additional custom actions from the payload
    if (notificationData.actions && Array.isArray(notificationData.actions)) {
        options.actions = [...(options.actions || []), ...notificationData.actions];
    }

    // Add any additional data from the payload
    if (notificationData.data) {
        options.data = { ...options.data, ...notificationData.data };
    }

    event.waitUntil(
        self.registration.showNotification(
            notificationData.title || 'Attendr',
            options
        )
    );
}, true); // Use capture phase to handle before NGSW

// Handle notification clicks
self.addEventListener('notificationclick', (event) => {
    event.notification.close();

    // Handle action clicks
    if (event.action) {
        let targetUrl = null;

        // Handle "Show More" action
        if (event.action === 'show-more') {
            targetUrl = event.notification.data?.url;
        } else {
            // Handle other custom actions
            const action = event.notification.data?.actions?.find(
                (a) => a.action === event.action
            );
            if (action && action.url) {
                targetUrl = action.url;
            }
        }

        if (targetUrl) {
            event.waitUntil(
                clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
                    // Check if a window with the target URL is already open
                    for (let client of clientList) {
                        if (client.url === targetUrl && 'focus' in client) {
                            return client.focus();
                        }
                    }
                    // If not, open a new window
                    if (clients.openWindow) {
                        return clients.openWindow(targetUrl);
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
