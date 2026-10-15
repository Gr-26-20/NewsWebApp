function Subscribe(id) {
    const url = `/Newsletter/Subscribe?userId=${id}`;
    fetch(url, {
        method: 'GET'
    })
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .catch(function (error) {
            console.error('Error updating feedback', error);
        })

    const button = document.getElementById(`subscribeButton`);
    localStorage.setItem('subscribed', 'true');
}

function CheckSubscriptionStatus(id) {
    if (checkSubscription()) {
    document.getElementById('subscribeButton').style.backgroundColor = 'green';
    Subscribe(id)
} else {
    document.getElementById('subscribeButton').style.backgroundColor = 'red';
    Subscribe(id)
}
    }

function checkSubscription() {
    return localStorage.getItem('subscribed') === 'true';
}

