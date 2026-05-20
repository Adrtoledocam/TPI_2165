import { pool } from "../config/db.mjs";

// GET -> /api/preferences -> Retourne les préférences de l'utilisateur connecté
export const getPreferences = async (req, res) => {
    const userId = req.user.id;

    try {
        const [rows] = await pool.execute(
            "SELECT preId, preCommunityOptIn, preUpdatedAt FROM t_preferences_user WHERE preUseId = ?",
            [userId]
        );

        if (rows.length === 0) {
            return res.status(404).json({ error: "Préférences introuvables." });
        }

        res.json({
            communityOptIn: rows[0].preCommunityOptIn === 1,
            updatedAt: rows[0].preUpdatedAt,
        });

    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors la récupération des préférences." });
    }
};

// PUT /api/preferences -> Active ou désactive la participation à la Communauté 
export const updatePreferences = async (req, res) => {
    const userId = req.user.id;
    const { communityOptIn } = req.body;

    if (communityOptIn === undefined || typeof communityOptIn !== "boolean") {
        return res.status(400).json({ error: "Le champ 'communityOptIn' est requis." });
    }

    try {
        const [result] = await pool.execute(
            "UPDATE t_preferences_user SET preCommunityOptIn = ? WHERE preUseId = ?",
            [communityOptIn, userId]
        );

        if (result.affectedRows === 0) {
            return res.status(404).json({ error: "Préférences introuvables." });
        }

        let message;
        if (communityOptIn) {
            message = "Vous participez maintenant à la Communauté.";
        } 
        else {
            message = "Vous avez quitté la Communauté.";
        }

        res.json({ message, communityOptIn });

    } catch (err) {
        res.status(500).json({ error: "Erreur serveur lors de la mise à jour des préférences." });
    }
};
