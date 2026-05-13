import mysql from 'mysql2/promise';
import dotenv from 'dotenv';

dotenv.config();

export const pool = mysql.createPool({
    host : process.env.DB_HOST,
    port : process.env.DB_PORT,
    user : process.env.DB_USER,
    password : process.env.DB_PASSWORD,
    database : process.env.DB_NAME,
    charset: 'utf8mb4',
    waitForConnections: true,
    connectionLimit: 10,
    queueLimit: 0
});

//Test fonction
export const testConnection = async () => {
    try{
        const connection = await pool.getConnection();
        console.log('Connecté à la base de données MySQL');
        connection.release();
    } catch (err) {
        console.error('Erruer de connexion à la base de données :', err.message);
    }
};
