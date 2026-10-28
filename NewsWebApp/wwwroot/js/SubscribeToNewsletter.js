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


function SubscribeToNewsCategory(id) {
    const url = `/Newsletter/SubscribeToNewsCategory?userId=${id}`;
    fetch(url, {
        method: 'GET'
    })
        .then(function(response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .catch(function(error) {
            console.error('Error updating feedback', error);
        })


    const subscriptionStatus =
        localStorage.getItem("subscribedToNews") === "true";

    const newStatus = !subscriptionStatus;

    localStorage.setItem("subscribedToNews", String(newStatus));

    const button = document.getElementById("subscribeNewsButton");

    // Red if subscribed, green if unsubscribed
    button.style.backgroundColor = newStatus ? "green" : "red";

    button.textContent = subscriptionStatus ? "Subscribed to news newsletter!" : "Unsubscribed from news newsletter";

}



function SubscribeToWorldCategory(id) {
    const url = `/Newsletter/SubscribeToWorldCategory?userId=${id}`;
    fetch(url, {
        method: 'GET'
    })
        .then(function(response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .catch(function(error) {
            console.error('Error updating feedback', error);
        })


    const subscriptionStatusWorld =
        localStorage.getItem("subscribedToWorld") === "true";

    const newStatus = !subscriptionStatusWorld;

    localStorage.setItem("subscribedToWorld", String(newStatus));

    const button = document.getElementById("subscribeWorldButton");

    // Red if subscribed, green if unsubscribed
    button.style.backgroundColor = newStatus ? "green" : "red";

    button.textContent = subscriptionStatusWorld ? "Subscribed to world newsletter!" : "Unsubscribed from world newsletter";

}


function SubscribeToSwedenCategory(id) {
    const url = `/Newsletter/SubscribeToSwedenCategory?userId=${id}`;
    fetch(url, {
        method: 'GET'
    })
        .then(function(response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .catch(function(error) {
            console.error('Error updating feedback', error);
        })


    const subscriptionStatusWorld =
        localStorage.getItem("subscribedToSweden") === "true";

    const newStatus = !subscriptionStatusWorld;

    localStorage.setItem("subscribedToSweden", String(newStatus));

    const button = document.getElementById("subscribeSwedenButton");

    // Red if subscribed, green if unsubscribed
    button.style.backgroundColor = newStatus ? "green" : "red";

    button.textContent = subscriptionStatusWorld ? "Subscribed to Sweden newsletter!" : "Unsubscribed from Sweden newsletter";

}


function SubscribeToWeatherCategory(id) {
    const url = `/Newsletter/SubscribeToWeatherCategory?userId=${id}`;
    fetch(url, {
        method: 'GET'
    })
        .then(function(response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .catch(function(error) {
            console.error('Error updating feedback', error);
        })


    const subscriptionStatusWeather =
        localStorage.getItem("subscribedToWeather") === "true";

    const newStatus = !subscriptionStatusWeather;

    localStorage.setItem("subscribedToWeather", String(newStatus));

    const button = document.getElementById("subscribeWeatherButton");

    // Red if subscribed, green if unsubscribed
    button.style.backgroundColor = newStatus ? "green" : "red";

    button.textContent = subscriptionStatusWeather ? "Subscribed to weather newsletter!" : "Unsubscribed from weather newsletter";

}


function SubscribeToSportsCategory(id) {
    const url = `/Newsletter/SubscribeToSportsCategory?userId=${id}`;
    fetch(url, {
        method: 'GET'
    })
        .then(function(response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .catch(function(error) {
            console.error('Error updating feedback', error);
        })


    const subscriptionStatusWeather =
        localStorage.getItem("subscribedToSports") === "true";

    const newStatus = !subscriptionStatusWeather;

    localStorage.setItem("subscribedToSports", String(newStatus));

    const button = document.getElementById("subscribeSportsButton");

    // Red if subscribed, green if unsubscribed
    button.style.backgroundColor = newStatus ? "green" : "red";

    button.textContent = subscriptionStatusWeather ? "Subscribed to Sports newsletter!" : "Unsubscribed from Sports newsletter";

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

    const subscriptionStatusNews =
        localStorage.getItem("subscribedToNews") === "true";

    const subscriptionStatusWorld =
        localStorage.getItem("subscribedToWorld") === "true";

    const subscriptionStatusSweden =
        localStorage.getItem("subscribedToSweden") === "true";

    const subscriptionStatusWeather =
        localStorage.getItem("subscribedToWeather") === "true";

    const subscriptionStatusSports =
        localStorage.getItem("subscribedToSports") === "true";
        

    const button = document.getElementById("subscribeButton");
    const newsButton = document.getElementById("subscribeNewsButton");
    const worldButton = document.getElementById("subscribeWorldButton");
    const swedenButton = document.getElementById("subscribeSwedenButton");
    const weatherButton = document.getElementById("subscribeWeatherButton");
    const sportsButton = document.getElementById("subscribeSportsButton");

    button.style.backgroundColor = subscriptionStatus ? "green" : "red";
    newsButton.style.backgroundColor = subscriptionStatusNews ? "green" : "red";
    worldButton.style.backgroundColor = subscriptionStatusWorld ? "green" : "red";
    swedenButton.style.backgroundColor = subscriptionStatusSweden ? "green" : "red";
    weatherButton.style.backgroundColor = subscriptionStatusWeather ? "green" : "red";
    sportsButton.style.backgroundColor = subscriptionStatusSports ? "green" : "red";
    

    button.textContent =
        subscriptionStatus ? "Unsubscribed" : "Subscribed!";
    newsButton.textContent =
        subscriptionStatusNews ? "Unsubscribed to news" : "Subscribed to news!";
    worldButton.textContent =
        subscriptionStatusWorld ? "Unsubscribed to world news" : "Subscribed to world news!";
    swedenButton.textContent =
        subscriptionStatusSweden ? "Unsubscribed to Sweden news" : "Subscribed to Sweden news!";
    weatherButton.textContent =
        subscriptionStatusWeather ? "Unsubscribed to weather news" : "Subscribed to weather news!";
    sportsButton.textContent =
        subscriptionStatusSports ? "Unsubscribed to sports news" : "Subscribed to sports news!";


    
});

