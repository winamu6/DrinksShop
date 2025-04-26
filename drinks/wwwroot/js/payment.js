document.addEventListener('DOMContentLoaded', function () {
    const coinInputs = document.querySelectorAll('.coin-input');
    const coinIncrements = document.querySelectorAll('.coin-increment');
    const coinDecrements = document.querySelectorAll('.coin-decrement');
    const insertedAmountSpan = document.getElementById('inserted-amount');
    const totalAmountSpan = document.getElementById('total-amount');
    const payButton = document.getElementById('pay-button');

    const totalAmount = parseFloat(totalAmountSpan.textContent.replace(' руб.', '').replace(',', '.')) || 0;
    let insertedAmount = 0;

    // Установка обработчиков событий
    coinIncrements.forEach(btn => {
        btn.addEventListener('click', () => {
            const input = btn.parentElement.querySelector('.coin-input');
            input.value = (parseInt(input.value) || 0) + 1;
            updateInsertedAmount();
        });
    });

    coinDecrements.forEach(btn => {
        btn.addEventListener('click', () => {
            const input = btn.parentElement.querySelector('.coin-input');
            const current = (parseInt(input.value) || 0);
            if (current > 0) {
                input.value = current - 1;
                updateInsertedAmount();
            }
        });
    });

    payButton.addEventListener('click', processPayment);

    function updateInsertedAmount() {
        insertedAmount = Array.from(coinInputs).reduce((sum, input) => {
            const denomination = parseFloat(input.dataset.denomination) || 0;
            const count = parseInt(input.value) || 0;
            return sum + (denomination * count);
        }, 0);

        insertedAmountSpan.textContent = insertedAmount.toFixed(2) + ' руб.';

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
            if (!result?.success) {
                throw new Error(result?.message || 'Неизвестная ошибка при оплате');
            }

            await clearCartAndRedirect(result.order.id, result.changeAmount, result.changeCoins);
        } catch (error) {
            console.error('Ошибка оплаты:', error);
            alert('Ошибка оплаты: ' + error.message);
        } finally {
            payButton.disabled = false;
            payButton.textContent = 'Оплатить';
        }
    }

    async function clearCartAndRedirect(orderId, changeAmount, changeCoins) {
        try {
            await fetch('/Cart/Clear', { method: 'POST' });

            const coinsParam = encodeURIComponent(JSON.stringify(changeCoins));
            window.location.href = `/Payment/Success/${orderId}?changeAmount=${changeAmount}&coins=${coinsParam}`;
        } catch (error) {
            console.error('Ошибка очистки корзины:', error);
        }
    }

    updateInsertedAmount();
});
