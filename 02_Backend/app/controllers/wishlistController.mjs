import { pool } from "../config/db.mjs";

// GET /api/wishlist -> Tous les jeux de la liste de souhaits
export const getWishlist = async (req, res) => {
    const userId = req.user.id;
    const sort = req.query.sort ?? "recent";
    const search = req.query.q?.trim() ?? null;

    //Definir les filtres
    let orderBy = "col.colAddedAt DESC";
    switch (sort) {
        case "date":
            orderBy = "gam.gamReleaseDate ASC";
            break;
        case "notes":
            orderBy = "gam.gamMetacritic DESC";
            break;
        case "az":
            orderBy = "gam.gamTitle ASC";
            break;
        case "za":
            orderBy = "gam.gamTitle DESC";
            break;
        case "recent":
        default:
            orderBy = "col.colAddedAt DESC";
            break;
    }

    try {
        let query = `
            SELECT
                col.colId,
                col.colStatus,
                col.colAddedAt,
                gam.gamId,
                gam.gamTitle,
                gam.gamCoverUrl,
                gam.gamDeveloper,
                gam.gamGenre,
                gam.gamPlatforms,
                gam.gamMetacritic,
                gam.gamReleaseDate
            FROM t_collection_game col
            LEFT JOIN t_game gam ON col.colGamId = gam.gamId
            WHERE col.colUseId = ? AND col.colStatus = 'wishlist'`;

        const params = [userId];
        if (search && search.length >= 3) {
            query += " AND gam.gamTitle LIKE ?";
            params.push(`%${search}%`);
        }

        query += ` ORDER BY ${orderBy}`;

        const [rows] = await pool.execute(query, params);
        res.json(rows);
    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors la récupération de la liste de souhaits." });
    }
};

// PUT /api/wishlist/:id/move -> Déplace un jeu de la wishlist vers la collection 
export const moveToCollection = async (req, res) => {
    const userId = req.user.id;
    const colId = parseInt(req.params.id);
    const { status } = req.body;

    const validStatus = ["acquis", "playing", "termine"];
    if (!status || !validStatus.includes(status)) {
        return res.status(400).json({ error: "Statut invalide" });
    }

    try {
        //Vérifier l'entrée et l'utilisateur
        const [rows] = await pool.execute(
            "SELECT colId FROM t_collection_game WHERE colId = ? AND colUseId = ? AND colStatus = 'wishlist'",
            [colId, userId]
        );

        if (rows.length === 0) {
            return res.status(404).json({ error: "Entrée introuvable dans votre liste" });
        }

        //Changer le statut
        await pool.execute(
            "UPDATE t_collection_game SET colStatus = ? WHERE colId = ?",
            [status, colId]
        );
        res.json({ message: "Jeu déplacé dans votre collection." });

    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors du déplacement du jeu." });
    }
};

// DELETE /api/wishlist/:id -> Supprime un jeu de la liste de souhaits
export const deleteFromWishlist = async (req, res) => {
    const userId = req.user.id;
    const colId  = parseInt(req.params.id);

    try {
        const [result] = await pool.execute(
            "DELETE FROM t_collection_game WHERE colId = ? AND colUseId = ? AND colStatus = 'wishlist'",
            [colId, userId]
        );
        if (result.affectedRows === 0) {
            return res.status(404).json({ error: "Entrée introuvable" });
        }
        res.json({ message: "Jeu retiré de la liste de souhaits." });

    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors la suppression." });
    }
};
