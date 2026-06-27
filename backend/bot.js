const TelegramBot = require('node-telegram-bot-api');
const database = require('./database');
require('dotenv').config();

const token = process.env.TELEGRAM_BOT_TOKEN;
const webAppUrl = process.env.WEBAPP_URL || 'https://localhost:3000'; // Default fallbacks

if (!token) {
    console.error('CRITICAL: TELEGRAM_BOT_TOKEN is not defined in environment variables.');
    process.exit(1);
}

// Create a bot that uses 'polling' to fetch new updates
const bot = new TelegramBot(token, { polling: true });

console.log('Telegram Bot listener started.');

// Handle /start command
bot.onText(/\/start/, async (msg) => {
    const chatId = msg.chat.id;
    const username = msg.from.username || msg.from.first_name || 'Player';

    // Ensure the user exists in database
    try {
        await database.ensureUser(chatId, username);
    } catch (e) {
        console.error('Error ensuring user in start command:', e.message);
    }

    bot.sendMessage(chatId, `Welcome to Flappy Knight, ${username}! Click the button below to start playing and earn your glory!`, {
        reply_markup: {
            inline_keyboard: [
                [
                    {
                        text: '🎮 Play Flappy Knight',
                        web_app: { url: webAppUrl }
                    }
                ]
            ]
        }
    });
});

// Handle pre-checkout queries (must be answered within 10 seconds)
bot.on('pre_checkout_query', async (query) => {
    console.log(`[PAYMENT] Received pre-checkout query: id=${query.id}, user=${query.from.id}, amount=${query.total_amount}`);
    try {
        await bot.answerPreCheckoutQuery(query.id, true);
        console.log(`[PAYMENT] Pre-checkout query ${query.id} approved.`);
    } catch (err) {
        console.error(`[PAYMENT] Failed to answer pre-checkout query ${query.id}:`, err.message);
        await bot.answerPreCheckoutQuery(query.id, false, { error_message: 'Payment verification failed. Please try again.' });
    }
});

// Handle successful payment completion
bot.on('successful_payment', async (msg) => {
    const chatId = msg.chat.id;
    const payment = msg.successful_payment;
    const payload = payment.invoice_payload;

    console.log(`[PAYMENT] Successful payment from user=${chatId}. Payload=${payload}, amount=${payment.total_amount} Stars.`);

    try {
        // Parse invoice payload
        if (payload.startsWith('lives:')) {
            const count = parseInt(payload.split(':')[1], 10);
            await database.addLives(chatId, count);
            console.log(`[REWARD] Granted ${count} lives to user ${chatId}`);
            bot.sendMessage(chatId, `🎉 Thank you! You have successfully purchased and received +${count} lives! Good luck on your next run!`);
        } else if (payload.startsWith('skin:')) {
            const skinName = payload.split(':')[1];
            await database.addSkin(chatId, skinName);
            console.log(`[REWARD] Granted skin '${skinName}' to user ${chatId}`);
            bot.sendMessage(chatId, `🎉 Thank you! You have successfully purchased the ${skinName.toUpperCase()} Aura skin! Equip it in the shop menu.`);
        } else if (payload === 'premium') {
            await database.setPremium(chatId, true);
            console.log(`[REWARD] Upgraded user ${chatId} to Premium`);
            bot.sendMessage(chatId, `🎉 Thank you! Your account has been upgraded to Premium (Ad-Free). Enjoy your clean gaming experience!`);
        } else {
            console.warn(`[PAYMENT] Unknown payment payload received: ${payload}`);
        }
    } catch (err) {
        console.error(`[PAYMENT] Error delivering rewards for payload ${payload}:`, err.message);
        bot.sendMessage(chatId, `⚠️ There was an error delivering your purchased item. Please contact support with invoice reference: ${payment.telegram_payment_charge_id}`);
    }
});

module.exports = bot;
