$(document).ready(function () {
    updateCartCount();
    updateCartTotal();

    $(document).on('click', '.add-to-cart', function () {
        const productId = $(this).data('product-id');
        const button = $(this);

        button.prop('disabled', true);

        $.post('/Cart/AddItem', { productId: productId })
            .done(function (response) {
                updateCartCount();
                toastr.success('Товар добавлен в корзину');
            })
            .fail(function () {
                toastr.error('Не удалось добавить товар в корзину');
            })
            .always(function () {
                button.prop('disabled', false);
            });
    });

    $(document).on('click', '.remove-item', function (e) {
        e.preventDefault();
        const productId = $(this).data('product-id');
        const button = $(this);

        button.prop('disabled', true);

        $.post('/Cart/RemoveItem', { productId: productId })
            .done(function (response) {
                $(`#cart-item-${productId}`).fadeOut(300, function () {
                    $(this).remove();
                    updateCartTotals();

                    if ($('tbody tr').length === 0) {
                        location.reload();
                    }
                });
                toastr.success('Товар удален из корзины');
            })
            .fail(function () {
                toastr.error('Не удалось удалить товар из корзины');
                button.prop('disabled', false);
            });
    });

    $(document).on('click', '.increase-quantity', function () {
        const productId = $(this).data('product-id');
        updateQuantity(productId, 1);
    });

    $(document).on('click', '.decrease-quantity', function () {
        const productId = $(this).data('product-id');
        updateQuantity(productId, -1);
    });

    function updateQuantity(productId, delta) {
        const endpoint = delta > 0 ? '/Cart/IncreaseQuantity' : '/Cart/DecreaseQuantity';
        const button = $(`[data-product-id="${productId}"].${delta > 0 ? 'increase' : 'decrease'}-quantity`);
        const row = $(`#cart-item-${productId}`);

        button.prop('disabled', true);

        $.post(endpoint, { productId: productId })
            .done(function (response) {
                if (response.success) {
                    row.find('.quantity-display').text(response.quantity);
                    row.find('.item-total').text(formatCurrency(response.total));
                    $('#cart-total').text(formatCurrency(response.cartTotal));
                    $('#cart-count').text(response.itemsCount);
                }
            })
            .fail(function () {
                toastr.error('Ошибка при обновлении количества');
            })
            .always(function () {
                button.prop('disabled', false);
            });
    }

    function updateCartCount() {
        $.get('/Cart/GetCount')
            .done(function (response) {
                $('#cart-count').text(response.count);
            });
    }

    function updateCartTotal() {
        $.get('/Cart/GetCartTotal')
            .done(function (total) {
                $('#cart-total').text(formatCurrency(total));
            });
    }

    function updateCartTotals() {
        updateCartCount();
        updateCartTotal();
    }

    function formatCurrency(amount) {
        return new Intl.NumberFormat('ru-RU', {
            style: 'currency',
            currency: 'RUB',
            minimumFractionDigits: 2
        }).format(amount);
    }
});