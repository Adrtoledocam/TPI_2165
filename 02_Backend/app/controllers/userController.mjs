import { pool } from "../config/db.mjs";

// GET -> /api/user/profile -> Retourne les infos du profil + les statistiques
export const getProfile = async (req, res) => {
    const userId = req.user.id;

    try {
        // Infos de l'utilisateur
        const [userRows] = await pool.execute(
            "SELECT useId, useUsername, useEmail, useCreatedAt FROM t_user WHERE useId = ?",[userId]);

        if (userRows.length === 0) {
            return res.status(404).json({ error: "Utilisateur introuvable." });
        }
        const user = userRows[0];

        // Calculer les stadistiques 
        const [statsRows] = await pool.execute(
            `SELECT
                COUNT(*) AS total,
                SUM(colStatus = 'acquis' OR colStatus = 'playing') AS acquis,
                SUM(colStatus = 'playing') AS playing,
                SUM(colStatus = 'termine') AS termine,
                SUM(colStatus = 'wishlist') AS wishlist
            FROM t_collection_game
            WHERE colUseId = ?`,
            [userId]
        );

        const stats = statsRows[0];
        res.json({
            id: user.useId,
            username: user.useUsername,
            email: user.useEmail,
            createdAt: user.useCreatedAt,
            stats: {
                total: stats.total    || 0,
                acquis: stats.acquis   || 0, // acquis + playing
                playing: stats.playing  || 0,
                termine: stats.termine  || 0,
                wishlist: stats.wishlist || 0,
            }
        });
    } catch (err) {
        res.status(500).json({error: "Erreur serveur lors de la récupération du profil."});
    }
};

// DELETE -> /api/user -> Supprime le compte
export const deleteAccount = async (req, res) => {
    const userId = req.user.id;

    try {
        //Vérifier le compte
        const [rows] = await pool.execute(
            "SELECT useId FROM t_user WHERE useId = ?",
            [userId]
        );

        if (rows.length === 0) {
            return res.status(404).json({ error: "Utilisateur introuvable." });
        }
        await pool.execute(
            "DELETE FROM t_user WHERE useId = ?",
            [userId]
        );
        res.json({ message: "Compte supprimé avec succès. Toutes vos données ont été effacées." });
    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors la suppression du compte." });
    }
};
