const express = require('express');
const cors = require('cors');
const database = require('./database');
const bot = require('./bot');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3000;

app.use(cors());
app.use(express.json());

// Fetch user profile
app.get('/api/user/:id', async (req, res) => {
    const telegramId = parseInt(req.params.id, 10);
    const username = req.query.username || 'Player';

    if (isNaN(telegramId)) {
        return res.status(400).json({ error: 'Invalid Telegram ID' });
    }

    try {
        const user = await database.ensureUser(telegramId, username);
        res.json(user);
    } catch (err) {
        console.error('Error fetching user:', err.message);
        res.status(500).json({ error: 'Database error' });
    }
});

// Equip a skin
app.post('/api/user/:id/equip-skin', async (req, res) => {
    const telegramId = parseInt(req.params.id, 10);
    const { skinName } = req.body;

    if (isNaN(telegramId) || !skinName) {
        return res.status(400).json({ error: 'Invalid payload' });
    }

    try {
        const user = await database.equipSkin(telegramId, skinName);
        res.json({ success: true, user });
    } catch (err) {
        console.error('Error equipping skin:', err.message);
        res.status(400).json({ error: err.message });
    }
});

// Subtract lives when player restarts or takes a hit in certain modes (if backend tracking is desired)
app.post('/api/user/:id/subtract-lives', async (req, res) => {
    const telegramId = parseInt(req.params.id, 10);
    const { amount } = req.body;

    if (isNaN(telegramId) || typeof amount !== 'number') {
        return res.status(400).json({ error: 'Invalid payload' });
    }

    try {
        const user = await database.subtractLives(telegramId, amount);
        res.json({ success: true, user });
    } catch (err) {
        console.error('Error subtracting lives:', err.message);
        res.status(500).json({ error: 'Database error' });
    }
});

// Create Telegram Stars invoice link
app.post('/api/create-invoice', async (req, res) => {
    const { telegram_id, item_type, item_id } = req.body;
    const userId = parseInt(telegram_id, 10);

    if (isNaN(userId) || !item_type) {
        return res.status(400).json({ error: 'Invalid request parameters' });
    }

    let title = '';
    let description = '';
    let payload = '';
    let price = 0;

    // Configure pricing
    if (item_type === 'lives') {
        const count = parseInt(item_id, 10);
        const pricingMap = {
            10: 100,
            20: 200,
            30: 300,
            60: 500,
            140: 1000
        };

        price = pricingMap[count];
        if (!price) {
            return res.status(400).json({ error: 'Invalid lives bundle count' });
        }

        title = `Buy +${count} Lives`;
        description = `Get ${count} additional lives to continue playing and setting new high scores in Flappy Knight!`;
        payload = `lives:${count}`;
    } else if (item_type === 'skin') {
        const skinName = item_id;
        const validSkins = ['red', 'yellow', 'green', 'blue', 'orange'];

        if (!validSkins.includes(skinName)) {
            return res.status(400).json({ error: 'Invalid skin name' });
        }

        price = 5000; // 5000 Stars
        title = `Buy ${skinName.toUpperCase()} Aura Skin`;
        description = `Unlock a beautiful, glowing ${skinName} aura effect around your knight during gameplay!`;
        payload = `skin:${skinName}`;
    } else if (item_type === 'premium') {
        price = 10000; // 10000 Stars
        title = 'Unlock Premium Status';
        description = 'Upgrade your account to Premium! This completely disables all advertisements for a clean, uninterrupted gameplay experience.';
        payload = 'premium';
    } else {
        return res.status(400).json({ error: 'Invalid item type' });
    }

    try {
        console.log(`[INVOICE] Generating invoice link for user=${userId}, payload=${payload}, price=${price} Stars`);
        
        // generate invoice link using Telegram Bot API
        const invoiceLink = await bot.createInvoiceLink(
            title,
            description,
            payload,
            '', // Provider token is empty for Telegram Stars
            'XTR', // Stars currency
            [{ label: 'Telegram Stars', amount: price }]
        );

        res.json({ success: true, invoiceLink });
    } catch (err) {
        console.error('Error generating invoice link:', err.message);
        res.status(500).json({ error: 'Failed to create invoice link' });
    }
});

// Start Express server
app.listen(PORT, () => {
    console.log(`Express server running on port ${PORT}`);
});
