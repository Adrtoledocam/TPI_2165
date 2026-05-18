import { pool } from "../config/db.mjs";

const RAWG_BASE_URL = process.env.RAWG_BASE_URL;
const RAWG_API_KEY  = process.env.RAWG_API_KEY;

// Map RAWG en objet -> t_game
const mapRawgGame = (gam) => ({
    id:          gam.id,
    title:       gam.name,
    coverUrl:    gam.background_image ?? null,
    platforms:   gam.platforms ?.map(p => p.platform.name).join("|") ?? null,
    developer:   gam.developers?.[0]?.name ?? null, //Juste pour le details du jeu
    publisher:   gam.publishers?.[0]?.name ?? null, //Juste pour le details du jeu
    genre:       gam.genres?.[0]?.name ?? null,
    metacritic:  gam.metacritic ?? null,
    releaseDate: gam.released ?? null,
});


//GET -> /api/games/trending -> Les premier joux à afficher avec bon note metacritic
export const getTrending = async (req, res) => {
    const useId = req.user.id;

    try {
        const url = `${RAWG_BASE_URL}/games?key=${RAWG_API_KEY}&ordering=-added&page_size=10`;
        const response = await fetch(url);

        if (!response.ok)
            return res.status(502).json({ error: "Erreur lors de la communication avec l'API RAWG." });

        const data = await response.json();
        const rawgGames = data.results;

        //Récupérer les IDs des jeux déjà dans la collection de l'utilisateur
        const [collectionRows] = await pool.execute(
            "SELECT colGamId FROM t_collection_game WHERE colUseId = ?", [useId]);
        const userGameIds = collectionRows.map(row => row.colGamId);

        //Construire la liste finale avec le inCollection
        const finalGames = [];
        for (const gam of rawgGames) {
            const mappedGame = mapRawgGame(gam);
            mappedGame.inCollection = userGameIds.includes(gam.id);
            finalGames.push(mappedGame);
        }

        res.json(finalGames);

    } catch (err) {
        console.error("Erreur getTrending :", err.message);
        res.status(500).json({ error: "Erreur serveur lors de la récupération des tendances." });
    }
};

//GET -> /api/games/search?
export const searchGames = async (req, res) => {
    const useId = req.user.id;
    const query = req.query.q?.trim();
    const page  = parseInt(req.query.page) || 1;
    if (!query || query.length < 3)
        return res.status(400).json({ error: "Paramètre requis (min. 3 caractères)" });

    try {
        const url = `${RAWG_BASE_URL}/games?key=${RAWG_API_KEY}&search=${encodeURIComponent(query)}&page_size=10&page=${page}`;
        const response = await fetch(url);

        if (!response.ok)
            return res.status(502).json({ error: "Erreur lors de la communication avec l'API RAWG." });

        const data = await response.json();
        const rawgGames = data.results;

        //Récupérer les IDs de la collection de l'utilisateur
        const [collectionRows] = await pool.execute(
            "SELECT colGamId FROM t_collection_game WHERE colUseId = ?", [useId]);
        const userGameIds = collectionRows.map(row => row.colGamId);

        //Liste finale avec le inCollection
        const finalGames = [];
        for (const gam of rawgGames) {
            const mappedGame = mapRawgGame(gam);
            mappedGame.inCollection = userGameIds.includes(gam.id);
            finalGames.push(mappedGame);
        }

        res.json({
            count: data.count,
            page,
            next: data.next ? page + 1 : null,
            previous: data.previous ? page - 1 : null,
            results:  finalGames,
        });

    } catch (err) {
        console.error("Erreur searchGames :", err.message);
        res.status(500).json({ error: "Erreur serveur lors de la recherche." });
    }
};

// GET -> /api/games/:id -> Détail complet d'un jeu 
export const getGameDetail = async (req, res) => {
    const useId = req.user.id;
    const gamId = parseInt(req.params.id);

    if (isNaN(gamId))
        return res.status(400).json({ error: "ID de jeu invalide." });

    try {
        const url = `${RAWG_BASE_URL}/games/${gamId}?key=${RAWG_API_KEY}`;
        const response = await fetch(url);

        if (response.status === 404)
            return res.status(404).json({ error: "Jeu introuvable sur RAWG." });

        if (!response.ok)
            return res.status(502).json({ error: "Erreur lors de la communication avec l'API RAWG." });

        const gam = await response.json();

        // Chercher si le jeu est déjà dans la collection de l'utilisateur
        const [rows] = await pool.execute(
            `SELECT colId, colStatus, colRating, colComment, colPlaytime, colOwnPlatforms
             FROM t_collection_game 
             WHERE colUseId = ? 
             AND colGamId = ?`,
            [useId, gamId]
        );

        const gameData = mapRawgGame(gam);
        gameData.description    = gam.description_raw ?? null;
        gameData.collectionEntry = rows.length > 0 ? rows[0] : null;

        res.json(gameData);


    } catch (err) {
        console.error("Erreur getGameDetail :", err.message);
        res.status(500).json({ error: "Erreur serveur lors de la récupération du jeu." });
    }
};
