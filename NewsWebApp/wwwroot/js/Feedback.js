function addToCart(productId) {

    const url = `/Editor/?productId=${productId}`;;
    fetch(url, {
        method: 'GET'
    })
        .then(function (response) {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(function (data) {
            // handle response data if needed
            console.log('AddToCart response:', data);
        })
        .catch(function (error) {
            console.error('Fetch error:', error);
        });
}

function UpdateFeedback(id) {
    const url = `/Editor/Delete?id=${id}`;
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
        });
}