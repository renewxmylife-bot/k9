const sqlite3 = require('sqlite3').verbose();
const path = require('path');
const dbPath = path.resolve(__dirname, 'game.db');

const db = new sqlite3.Database(dbPath, (err) => {
    if (err) {
        console.error('Error opening database:', err.message);
    } else {
        console.log('Connected to SQLite database.');
        db.serialize(() => {
            db.run(`CREATE TABLE IF NOT EXISTS users (
                telegram_id INTEGER PRIMARY KEY,
                username TEXT,
                lives INTEGER DEFAULT 5,
                skins TEXT DEFAULT '["default"]',
                active_skin TEXT DEFAULT 'default',
                is_premium INTEGER DEFAULT 0
            )`);
        });
    }
});

// Helper functions wrapped in Promises
const dbGet = (sql, params = []) => {
    return new Promise((resolve, reject) => {
        db.get(sql, params, (err, row) => {
            if (err) reject(err);
            else resolve(row);
        });
    });
};

const dbRun = (sql, params = []) => {
    return new Promise((resolve, reject) => {
        db.run(sql, params, function (err) {
            if (err) reject(err);
            else resolve(this);
        });
    });
};

const database = {
    async getUser(telegramId) {
        let user = await dbGet('SELECT * FROM users WHERE telegram_id = ?', [telegramId]);
        if (user) {
            user.skins = JSON.parse(user.skins || '["default"]');
            user.is_premium = !!user.is_premium;
        }
        return user;
    },

    async ensureUser(telegramId, username) {
        let user = await this.getUser(telegramId);
        if (!user) {
            await dbRun(
                'INSERT INTO users (telegram_id, username, lives, skins, active_skin, is_premium) VALUES (?, ?, 5, ?, "default", 0)',
                [telegramId, username || '', JSON.stringify(['default'])]
            );
            user = await this.getUser(telegramId);
        }
        return user;
    },

    async addLives(telegramId, amount) {
        await dbRun('UPDATE users SET lives = lives + ? WHERE telegram_id = ?', [amount, telegramId]);
        return this.getUser(telegramId);
    },

    async subtractLives(telegramId, amount) {
        let user = await this.getUser(telegramId);
        if (user) {
            const newLives = Math.max(0, user.lives - amount);
            await dbRun('UPDATE users SET lives = ? WHERE telegram_id = ?', [newLives, telegramId]);
        }
        return this.getUser(telegramId);
    },

    async addSkin(telegramId, skinName) {
        let user = await this.getUser(telegramId);
        if (user) {
            const skins = user.skins;
            if (!skins.includes(skinName)) {
                skins.push(skinName);
                await dbRun('UPDATE users SET skins = ? WHERE telegram_id = ?', [JSON.stringify(skins), telegramId]);
            }
        }
        return this.getUser(telegramId);
    },

    async equipSkin(telegramId, skinName) {
        let user = await this.getUser(telegramId);
        if (user) {
            const skins = user.skins;
            if (skins.includes(skinName)) {
                await dbRun('UPDATE users SET active_skin = ? WHERE telegram_id = ?', [skinName, telegramId]);
            } else {
                throw new Error(`Skin ${skinName} not purchased.`);
            }
        }
        return this.getUser(telegramId);
    },

    async setPremium(telegramId, isPremium) {
        await dbRun('UPDATE users SET is_premium = ? WHERE telegram_id = ?', [isPremium ? 1 : 0, telegramId]);
        return this.getUser(telegramId);
    }
};

module.exports = database;
