import { pool } from "../config/db.mjs";

const RAWG_BASE_URL = process.env.RAWG_BASE_URL;
const RAWG_API_KEY  = process.env.RAWG_API_KEY;

// Map RAWG en objet -> t_game
const mapRawgGame = (gam) => ({
    id:          gam.id,
    title:       gam.name,
    coverUrl:    gam.background_image ?? null,
    platforms:   gam.platforms ?.map(p => p.platform.name).join("|") ?? null,
    developer:   gam.developers?.[0]?.name ?? null, //Juste dans le details du jeu
    publisher:   gam.publishers?.[0]?.name ?? null, //Juste dans le details du jeu
    genre:       gam.genres?.[0]?.name ?? null,
    metacritic:  gam.metacritic ?? null,
    releaseDate: gam.released ?? null,
});

const getFilterOrdering = (sort) => {
    let ordering = "-metacritic";
    let datesFilter = "";

    const today = new Date().toISOString().split("T")[0];
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const tomorrowStr = tomorrow.toISOString().split("T")[0];

    switch (sort) {
        case "recent":
            ordering = "-released";
            datesFilter = `&dates=1990-01-01,${today}`;
            break;
        case "az":
            //ordering = "name";
            break;
        case "za":
            //ordering = "-name";
            break;
        case "avenir":
            ordering = "released";
            //const today = new Date().toISOString().split("T")[0];
            datesFilter = `&dates=${tomorrowStr},2030-12-31`;
            break;
        case "notes":
        default:
            ordering = "-metacritic";
            break;
    }
    return { ordering, datesFilter };
};

// Tri local A-Z ou Z-A 
const sortGames = (games, sort) => {
    if (sort === "az" || sort === "za") {
        const filtered = games.filter(g => /^[A-Za-z0-9]/.test(g.title));
        if (sort === "az") return filtered.sort((a, b) => a.title.localeCompare(b.title));
        if (sort === "za") return filtered.sort((a, b) => b.title.localeCompare(a.title));
    }
    return games;
};

//GET -> /api/games/trending -> Les premier joux à afficher avec bon note metacritic
export const getTrending = async (req, res) => {
    const useId = req.user.id;
    const sort   = req.query.sort ?? "notes";
    const page  = parseInt(req.query.page) || 1; 

    const { ordering, datesFilter } = getFilterOrdering(sort);

    try {
        const url = `${RAWG_BASE_URL}/games?key=${RAWG_API_KEY}&ordering=${ordering}&page_size=20${datesFilter}&page=${page}`;
        const response = await fetch(url);

        if (!response.ok)
            return res.status(502).json({ error: "Erreur avec l'API RAWG." });

        const data = await response.json();
        
        //Récupérer les IDs des jeux déjà dans la collection de l'utilisateur
        const [collectionRows] = await pool.execute(
            "SELECT colGamId FROM t_collection_game WHERE colUseId = ?", [useId]);
        const userGameIds = collectionRows.map(row => row.colGamId);

        //Construire la liste finale avec le inCollection
        const finalGames = [];
        for (const gam of data.results) {
            const mappedGame = mapRawgGame(gam);
            mappedGame.inCollection = userGameIds.includes(gam.id);
            finalGames.push(mappedGame);
        }

        /* Tri local
        if (sort === "az" || sort === "za") { //Solution a bug des titres en autre langue
            sortGames(finalGames, sort);
        }*/

        if (sort === "az") {
            finalGames.sort((a, b) => a.title.localeCompare(b.title));
        }
        if (sort === "za") {
            finalGames.sort((a, b) => b.title.localeCompare(a.title));
        }

        res.json(finalGames);

    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors de la récupération des tendances." });
    }
};

//GET -> /api/games/search?
export const searchGames = async (req, res) => {
    const useId = req.user.id;
    const query = req.query.q?.trim();
    const page  = parseInt(req.query.page) || 1;
    const sort = req.query.sort ?? "notes";
    if (!query || query.length < 3)
        return res.status(400).json({ error: "Recherche trop courte." });

    const { ordering, datesFilter } = getFilterOrdering(sort);

    try {
        const url = `${RAWG_BASE_URL}/games?key=${RAWG_API_KEY}&search=${encodeURIComponent(query)}&ordering=${ordering}&page_size=20&page=${page}${datesFilter}`;
        const response = await fetch(url);

        if (!response.ok)
            return res.status(502).json({ error: "Erreur avec RAWG." });

        const data = await response.json();
        let rawgGames = data.results;

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
        res.status(500).json({ error: "Erreur serveur lors de la récupération du jeu." });
    }
};
