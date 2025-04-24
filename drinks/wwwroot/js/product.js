$(document).ready(function () {
    function checkCartItems() {
        $.get('/Cart/GetCartItems')
            .done(function (response) {
                $('.card').removeClass('card-in-cart');

                response.forEach(function (item) {
                    const $button = $(`.add-to-cart[data-product-id="${item.productId}"]`);
                    $button.closest('.card').addClass('card-in-cart');
                    $button.text('Добавлено').prop('disabled', true);
                });
            });
    }

    function updateCartCount() {
        $.get('/Cart/GetCount')
            .done(function (response) {
                $('#cart-count').text(response.count);
                $('#cart-button').toggleClass('disabled', response.count === 0);
                checkCartItems();
            });
    }

    updateCartCount();

    $(document).on('cartUpdated', function () {
        updateCartCount();
    });

    $('#brand-select').change(function () {
        filterProducts();
    });

    $('#price-slider').on('input', function () {
        $('#price-value').text($(this).val() + ' руб.');
        filterProducts();
    });

    function filterProducts() {
        const brandId = $('#brand-select').val();
        const maxPrice = $('#price-slider').val();
        const minPrice = $('#price-slider').attr('min');

        $.get('/Products/Filter', { brandId, minPrice, maxPrice })
            .done(function (html) {
                $('#products-container').html(html);
                checkCartItems();
            })
            .fail(function () {
                toastr.error('Ошибка при фильтрации товаров');
            });
    }


    $(document).on('click', '.add-to-cart', function () {
        const productId = $(this).data('product-id');
        const $button = $(this);
        const $card = $button.closest('.card');

        $.ajax({
            url: '/Cart/AddItem',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                localStorage.setItem('cartCount', response.count);
                $('#cart-count').text(response.count);
                $card.addClass('card-in-cart');
                $button.text('Добавлено').prop('disabled', true);
                toastr.success('Товар добавлен в корзину');
                $(document).trigger('cartUpdated');
            },
            error: function (xhr) {
                console.error('Error:', xhr.responseText);
                toastr.error('Ошибка при добавлении в корзину');
            }
        });
    });
});