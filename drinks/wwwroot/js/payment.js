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
                const error = await response.json().catch(() => ({ message: 'Неизвестная ошибка сервера' }));
                throw new Error(error.message);
            }

            const result = await response.json();

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
                </div>
            `);
        } finally {
            payButton.disabled = false;
            payButton.textContent = 'Оплатить';
        }
    }

    function handleSuccessfulPayment(result) {
        let message = '<div class="alert alert-success"><h4>Спасибо за покупку!</h4>';

        if (result.changeAmount > 0) {
            message += `<p>Ваша сдача: <strong>${result.changeAmount.toFixed(2)} руб.</strong></p>`;

            if (result.changeCoins && Object.keys(result.changeCoins).length > 0) {
                message += '<p>Монеты для сдачи:</p><ul class="mb-0">';
                for (const [denomination, count] of Object.entries(result.changeCoins)) {
                    message += `<li>${denomination} руб. × ${count}</li>`;
                }
                message += '</ul>';
            }
        } else {
            message += '<p>Сдача не требуется.</p>';
        }

        message += '</div>';
        showPaymentResult(message);

        clearCartAndRedirect(result.order.id);
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

    async function clearCartAndRedirect(orderId) {
        try {
            await fetch('/Cart/Clear', { method: 'POST' });
            paymentResultModal._element.addEventListener('hidden.bs.modal', () => {
                window.location.href = `/Payment/Success/${orderId}`;
            }, { once: true });
        } catch (error) {
            console.error('Failed to clear cart:', error);
        }
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

    initEventListeners();
    updateInsertedAmount();
});