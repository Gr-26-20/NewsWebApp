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


        const subscriptionStatus =
           localStorage.getItem("subscribed") === "true";

       const newStatus = !subscriptionStatus;

       localStorage.setItem("subscribed", String(newStatus));

       const button = document.getElementById("subscribeButton");

       // Red if subscribed, green if unsubscribed
       button.style.backgroundColor = newStatus ? "red" : "green";
       
       button.textContent = subscriptionStatus ? "Subscribed!" : "Unsubscribed";

}



function CheckSubscriptionStatus(id) {
    Subscribe(id)
}

function checkSubscription() {
    return localStorage.getItem('subscribed') === 'true';
}


document.addEventListener("DOMContentLoaded", function () {
    const subscriptionStatus =
        localStorage.getItem("subscribed") === "true";

    const button = document.getElementById("subscribeButton");

    button.style.backgroundColor = subscriptionStatus ? "red" : "green";

    button.textContent =
            subscriptionStatus ? "Unsubscribed" : "Subscribed!";
});

