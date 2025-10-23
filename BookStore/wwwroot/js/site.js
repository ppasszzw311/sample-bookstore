(() => {
    const subscriptionForm = document.querySelector('[data-subscription-form]');
    if (!subscriptionForm) {
        return;
    }

    subscriptionForm.addEventListener('submit', () => {
        const submitButton = subscriptionForm.querySelector('button[type="submit"]');
        if (submitButton) {
            submitButton.setAttribute('disabled', 'disabled');
            submitButton.dataset.originalText = submitButton.innerText;
            submitButton.innerText = '訂閱中...';
        }
    });
})();
