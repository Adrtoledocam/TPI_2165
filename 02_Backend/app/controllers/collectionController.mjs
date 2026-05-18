import {pool} from "../config/db.mjs";

//GET -> /api/collection -> Retourne la collection de l'user
export const getCollection = async (req, res) => {
    const useId = req.user.id;
    try{
        const [rows] = await pool.execute(
            `SELECT
                col.colId,
                col.colStatus,
                col.colRating,
                col.colComment,
                col.colPlaytime,
                col.colOwnPlatforms,
                col.colAddedAt,
                gam.gamId,
                gam.gamTitle,
                gam.gamCoverUrl,
                gam.gamDeveloper,
                gam.gamPublisher,
                gam.gamGenre,
                gam.gamPlatforms,
                gam.gamMetacritic,
                gam.gamReleaseDate
            FROM t_collection_game col
            LEFT JOIN t_game gam ON col.colGamId = gam.gamId
            WHERE col.colUseId = ?
            ORDER BY col.colAddedAt DESC`,
            [useId]
        );
        res.json(rows);      
    } catch (err) {
        console.error("Erreur getCollection :", err.message);
        res.status(500).json({ error: "Erreur serveur lors de la récupération de la collection." });
    }
};

//POST -> api/collection -> Ajoute un jeu à la collection
export const addToCollection = async (req, res) =>{
    const useId = req.user.id;
    const {game, status, ownPlatforms} = req.body;

    //Verifier le formulaire + status
    if (!game || !game.id || !status)
        return res.status(400).json({ error: "Champs requis manquants (game, status)"});

    const validSatatus = ["aquis", "playing", "termine", "wishlist"];
    if (!validSatatus.includes(status))
        return res.status(400).json({error : `Statut invalide`});

    try {
        //Verifier si le jeu est déjà dans la collection
        const [existing] = await pool.execute(
            "SELECT colId FROM t_collection_game WHERE colUseId = ? AND colGamId = ?", [useId, game.id]);
        if (existing.length>0)
            return res.status(409).json({ error: "Ce jeu est déjà dans votre collection." });

        //Insert sur le cache le t_game
        await pool.execute(
            `INSERT IGNORE INTO t_game (gamId, gamTitle, gamCoverUrl, gamPlatforms, gamDeveloper, gamPublisher, gamGenre, gamMetacritic, gamReleaseDate)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)`,
            [
                game.id,
                game.title ?? null,
                game.coverUrl ?? null,
                game.platforms ?? null,
                game.developer ?? null,
                game.publisher ?? null,
                game.genre ?? null,
                game.metacritic ?? null,
                game.releaseDate ?? null,
            ]
        );

        //Insert sur la collection.
        const [result] = await pool.execute(
            `INSERT INTO t_collection_game (colStatus, colOwnPlatforms, colUseId, colGamId)
            VALUES (?, ?, ?, ?)`, [status, ownPlatforms ?? null, useId, game.id]
        );
        res.status(201).json({ message: "Jeu ajouté à la collection.", colId: result.insertId });
    } catch (err) {
        console.error("Erreur addToCollection :", err.message);
        res.status(500).json({ error: "Erreur serveur lors de l'ajout du jeu." });
    }
} 

// PUT -> /api/collection/:id -> Modifier le satatus, note, commentaire, temps jeu et platf.
export const updateCollectionEntry = async (req, res) => {
    const userId = req.user.id;
    const colId = parseInt(req.params.id);
    const { status, rating, comment, playtime, ownPlatforms } = req.body;

    const validStatus = ["acquis", "playing", "termine", "wishlist"];
    if (status && !validStatus.includes(status)) {
        return res.status(400).json({ error: "Statut invalide." });
    }

    if (rating !== undefined && (rating < 1 || rating > 5)) {
        return res.status(400).json({ error: "La note doit être entre 1 et 5." });
    }

    try {
        //Vérifier l'entrée 
        const [rows] = await pool.execute(
            "SELECT colId FROM t_collection_game WHERE colId = ? AND colUseId = ?",
            [colId, userId]
        );

        if (rows.length === 0) {
            return res.status(404).json({ error: "Entrée introuvable dans votre collection." });
        }

        //Mettre à jour les champs fournis
        if (status !== undefined) {
            await pool.execute("UPDATE t_collection_game SET colStatus = ? WHERE colId = ?", [status, colId]);
        }
        if (rating !== undefined) {
            await pool.execute("UPDATE t_collection_game SET colRating = ? WHERE colId = ?", [rating, colId]);
        }
        if (comment !== undefined) {
            await pool.execute("UPDATE t_collection_game SET colComment = ? WHERE colId = ?", [comment, colId]);
        }
        if (playtime !== undefined) {
            await pool.execute("UPDATE t_collection_game SET colPlaytime = ? WHERE colId = ?", [playtime, colId]);
        }
        if (ownPlatforms !== undefined) {
            await pool.execute("UPDATE t_collection_game SET colOwnPlatforms = ? WHERE colId = ?", [ownPlatforms, colId]);
        }

        res.json({ message: "Mise à jour avec succès." });

    } catch (err) {
        console.error("Erreur updateCollectionEntry :", err.message);
        res.status(500).json({ error: "Erreur serveur lors de la mise à jour." });
    }
};

//DELETE -> /api/collection/:id ->Supprimir une jeu de la collection
export const deleteCollectionEntry = async (req, res) => {
    const useId = req.user.id;
    const colId = parseInt(req.params.id);

    try {
        const [result] = await pool.execute(
            "DELETE FROM t_collection_game WHERE colId = ? AND colUseId = ?",
            [colId, useId]
        );

        if (result.affectedRows === 0)
            return res.status(404).json({ error: "Entrée introuvable dans la collection" });

        res.json({ message: "Jeu retiré de la collection." });
    } catch (err) {
        console.error("Erreur deleteCollectionEntry :", err.message);
        res.status(500).json({ error: "Erreur serveur lors de la suppression." });
    }
};