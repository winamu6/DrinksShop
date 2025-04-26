document.addEventListener('DOMContentLoaded', function () {

    const coinInputs = document.querySelectorAll('.coin-input');
    const coinIncrements = document.querySelectorAll('.coin-increment');
    const coinDecrements = document.querySelectorAll('.coin-decrement');
    const insertedAmountSpan = document.getElementById('inserted-amount');
    const totalAmountSpan = document.getElementById('total-amount');
    const payButton = document.getElementById('pay-button');
    const paymentResultModal = new bootstrap.Modal(document.getElementById('paymentResultModal'));
    const paymentResultMessage = document.getElementById('paymentResultMessage');
    const returnToCatalogBtn = document.getElementById('return-to-catalog');

    const totalAmount = parseFloat(totalAmountSpan.textContent);
    let insertedAmount = 0;

    function initEventListeners() {
        coinInputs.forEach(input => {
            input.addEventListener('input', updateInsertedAmount);
        });

        coinIncrements.forEach(btn => {
            btn.addEventListener('click', function () {
                const input = this.parentElement.querySelector('.coin-input');
                input.value = parseInt(input.value || 0) + 1;
                input.dispatchEvent(new Event('input'));
            });
        });

        coinDecrements.forEach(btn => {
            btn.addEventListener('click', function () {
                const input = this.parentElement.querySelector('.coin-input');
                const currentValue = parseInt(input.value || 0);
                if (currentValue > 0) {
                    input.value = currentValue - 1;
                    input.dispatchEvent(new Event('input'));
                }
            });
        });

        returnToCatalogBtn?.addEventListener('click', function () {
            window.location.href = '/';
        });

        payButton.addEventListener('click', processPayment);
    }

    function updateInsertedAmount() {
        insertedAmount = Array.from(coinInputs).reduce((sum, input) => {
            const denomination = parseFloat(input.dataset.denomination);
            const count = parseInt(input.value) || 0;
            return sum + denomination * count;
        }, 0);

        insertedAmountSpan.textContent = insertedAmount.toFixed(2);

        const hasEnoughMoney = insertedAmount >= totalAmount;
        insertedAmountSpan.classList.toggle('text-danger', !hasEnoughMoney);
        insertedAmountSpan.classList.toggle('text-success', hasEnoughMoney);
        payButton.disabled = !hasEnoughMoney;
    }

    async function processPayment() {
        const insertedCoins = Array.from(coinInputs).reduce((coins, input) => {
            const denomination = parseInt(input.dataset.denomination);
            const count = parseInt(input.value) || 0;
            if (count > 0) coins[denomination] = count;
            return coins;
        }, {});

        try {
            payButton.disabled = true;
            payButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Обработка...';

            const response = await fetch('/api/PaymentApi', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify(insertedCoins)
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => null);
                throw new Error(errorData?.message || `HTTP error! status: ${response.status}`);
            }

            const result = await response.json();

            if (!result) {
                throw new Error("Пустой ответ от сервера");
            }

            if (result.success) {
                handleSuccessfulPayment(result);
            } else {
                handleFailedPayment(result);
            }
        } catch (error) {
            console.error('Payment error:', error);
            showPaymentResult(`
                <div class="alert alert-danger">
                    <h5>Ошибка при обработке платежа</h5>
                    <p>${error.message}</p>
                    <p class="small">Попробуйте ещё раз или обратитесь в поддержку</p>
                </div>
            `);
        } finally {
            payButton.disabled = false;
            payButton.textContent = 'Оплатить';
        }
    }
    function handleSuccessfulPayment(result) {
        clearCartAndRedirect(result.order.id, result.changeAmount, result.changeCoins);
    }

    function handleFailedPayment(result) {
        const errorMessage = result.requiresExactChange
            ? `${result.message}<br><br>Попробуйте внести другую комбинацию монет.`
            : result.message;

        showPaymentResult(`<div class="alert alert-danger">${errorMessage}</div>`);
    }

    function showPaymentResult(content) {
        paymentResultMessage.innerHTML = content;
        paymentResultModal.show();
    }

    async function clearCartAndRedirect(orderId, changeAmount, changeCoins) {
        try {
            await fetch('/Cart/Clear', { method: 'POST' });

            const coinsParam = encodeURIComponent(JSON.stringify(changeCoins));
            window.location.href = `/Payment/Success/${orderId}?changeAmount=${changeAmount}&coins=${coinsParam}`;
        } catch (error) {
            console.error('Failed to clear cart:', error);
        }
    }

    initEventListeners();
    updateInsertedAmount();
});
