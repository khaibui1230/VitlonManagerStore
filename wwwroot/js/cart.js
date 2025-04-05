function updateCartIcon() {
    $.get('/Cart/GetCartInfo', function(data) {
        $('#cartItemCount').text(data.itemCount);
        $('#cartTotal').text(new Intl.NumberFormat('vi-VN', { 
            style: 'currency', 
            currency: 'VND' 
        }).format(data.total));
    });
}

$(document).ready(function() {
    // Update cart icon every 30 seconds
    setInterval(updateCartIcon, 30000);
});