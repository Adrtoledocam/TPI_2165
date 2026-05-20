import { pool } from "../config/db.mjs";

// GET /api/community -> Retourne la liste des utilisateurs avec la Communauté
export const getCommunity = async (req, res) => {
    try {
        //Vérifier l'accées 
        const [prefRows] = await pool.execute(
            "SELECT preCommunityOptIn FROM t_preferences_user WHERE preUseId = ?",
            [userId]
        );

        if (prefRows.length === 0 || prefRows[0].preCommunityOptIn !== 1) {
            return res.status(403).json({
                error: "Accès refusé. Activez la participation à la Communauté dans vos paramètres pour voir cette page."
            });
        }
        
        // Récupérer tous les utilisateur
        const [users] = await pool.execute(
            `SELECT u.useId, u.useUsername
             FROM t_user u
             INNER JOIN t_preferences_user p ON p.preUseId = u.useId
             WHERE p.preCommunityOptIn = TRUE
             ORDER BY u.useUsername ASC`
        );

        if (users.length === 0) {
            return res.json([]);
        }

        //Pour chaque utilisateur avec ses stats
        const result = [];

        for (const user of users) {
            const [statsRows] = await pool.execute(
                `SELECT
                    SUM(colStatus = 'acquis' OR colStatus = 'playing') AS acquis,
                    SUM(colStatus = 'termine') AS termine,
                    SUM(colStatus = 'wishlist') AS wishlist
                FROM t_collection_game
                WHERE colUseId = ?`,
                [user.useId]
            );

            const stats = statsRows[0];

            result.push({
                username: user.useUsername,
                stats: {
                    acquis: stats.acquis   || 0,
                    termine: stats.termine  || 0,
                    wishlist: stats.wishlist || 0,
                }
            });
        }
        res.json(result);

    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors la récupération de la communauté." });
    }
};
