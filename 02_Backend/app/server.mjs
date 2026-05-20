import express from 'express';
import cors from 'cors';
import dotenv from 'dotenv';
import { fileURLToPath } from 'url';
import { dirname, join } from 'path';

import authRoutes from './routes/authRoutes.mjs';
import collectionRoutes from './routes/collectionRoutes.mjs';
import gameRoutes from './routes/gameRoutes.mjs';
import userRoutes       from './routes/userRoutes.mjs';
import wishlistRoutes   from './routes/wishlistRoutes.mjs';
import preferencesRoutes   from './routes/preferencesRoutes.mjs';
import communityRoutes     from './routes/communityRoutes.mjs';
 
const __dirname = dirname(fileURLToPath(import.meta.url));
dotenv.config({ path: join(__dirname, '../.env') });

const REQUIRED_ENV = ['DB_HOST', 'DB_USER', 'DB_PASSWORD', 'DB_NAME', 'JWT_SECRET','RAWG_API_KEY', 'RAWG_BASE_URL'];
const missing = REQUIRED_ENV.filter(key => !process.env[key]);
if (missing.length > 0) {
    console.error("Variables d'env manquantes");
    process.exit(1);
}

const app = express();
const PORT = process.env.PORT||8080;

//Middlewares
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));

//Healthcheck
app.get('/', (req, res) => {
  res.json({ message: "Bienvenue sur l'API Arcaludo !", status: "Running" });
});

///ROUTES
app.use('/api/auth', authRoutes);
app.use('/api/collection', collectionRoutes);
app.use('/api/games', gameRoutes);
app.use('/api/user', userRoutes);
app.use('/api/wishlist', wishlistRoutes);
app.use('/api/preferences', preferencesRoutes);
app.use('/api/community', communityRoutes);

app.listen(PORT, () => {
  console.log(`Serveur démarré sur http://localhost:${PORT}`);
  console.log(`En attente de requêtes de l'application MAUI...`);
});