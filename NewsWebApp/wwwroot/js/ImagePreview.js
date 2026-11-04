function previewImage(input, imgId) 
{
    const preview = document.getElementById(imgId);

    if (input.files && input.files[0]){
        const file = input.files[0];

        if(!file.type.startsWith('image/')) {
            alert('Please select a valid image file.');
            input.value = ''; // Clear the input
            return;
        }
        //Temporary URL for the selected image file
        const imageUrl = URL.createObjectURL(file);
        preview.src = imageUrl;
    }

}

const logoPreview = document.getElementById('logoPreview');
const imageUrlPreview = document.getElementById('imageUrlPreview');
const imageUrl2Preview = document.getElementById('imageUrl2Preview');
const imageUrl3Preview = document.getElementById('imageUrl3Preview');

const logoInput = document.getElementById('Logo').addEventListener('change', function() {
    previewImage(this, 'logoPreview');
});

const imageUrlInput =document.getElementById('imageUrl').addEventListener('change', function() {
    previewImage(this, 'imageUrlPreview');
});

const imageUrl2Input = document.getElementById('imageUrl2').addEventListener('change', function() {
    previewImage(this, 'imageUrl2Preview');
});
const imageUrl3Input = document.getElementById('imageUrl3').addEventListener('change', function() {
    previewImage(this, 'imageUrl3Preview');
});
