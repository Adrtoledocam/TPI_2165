import {pool} from "../config/db.mjs";
import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";

// POST -> /api/auth/register
export const register = async (req, res) => {
    const {username, email, password} = req.body;

    // Validation des champs requis
    if (!username || !password || !email)
        return res.status(400).json({error:"Des champs requis sont manquants"});

    // Username min 3 et max 50 caractères
    if(username.length <3 || username.length >50)
        return res.status(400).json({error: "Le nom d'utilisateur doit contenir entre 3 et 50 caractères."})
    
    // Email avec le format "xxxx@xxxx.xx"
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email))
        return res.status(400).json({error: "Le format de l'adresse email est invalide"})

    // MDP avec 1 majus. 1min. 1 chiffre et 1 symbole
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-+_!@#$%^&*.,?]).{8,}$/;    
    if (!passwordRegex.test(password)) 
        return res.status(400).json({ error: "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule, un chiffre et un symbole."});
    
    try{
        //Verification d'email s'il exist
        const [existingUser] = await pool.execute('SELECT * FROM t_user WHERE useEmail = ?',[email]);
        if(existingUser.length>0){
            return res.status(400).json({message: "Cet email est déjà utilisé."})
        }

        //Hash sur email
        const salt =  await bcrypt.genSalt(10);
        const hashed = await bcrypt.hash(password, salt);

        //New user
        const [result] = await pool.execute(
            'INSERT INTO t_user (useUsername, useEmail, usePasswordHash) VALUES (?, ?, ?)', [username, email, hashed]);

        // Créer les préférences
        await pool.execute(
            'INSERT INTO t_preferences_user (preUseId) VALUES (?)',
            [result.insertId]
        );
        res.json ({message : "Utilisateur créé avec succès !"});
    } catch (err) {
        res.status(500).json({error: "Erreur serveur lors de l'inscription."});
    }
}; 

// POST /api/auth/login
export const login = async (req, res) => {
    const {email, password} = req.body;

    //Validation formulaire
    if (!email || !password)
        return res.status(400).json({ error: "Email et mot de passe requis." });
    try {
        const [users] = await pool.execute("SELECT * FROM t_user WHERE useEmail = ?", [email]);

        if (users.length === 0)
            return res.status(401).json({ error: "Cet utilisateur n'est pas enregistré" });

        const user = users[0];
        //MDP verification
        if (!user || !(await bcrypt.compare(password, user.usePasswordHash))){
            return res.status(401).json({ message : "Identifiants invalides."});
        }
        
        const token = jwt.sign(
            {
                id: user.useId,
            },
            process.env.JWT_SECRET,
            { expiresIn: "2h" }
        );

        res.json({ token, user: { id: user.useId, username: user.useUsername, email: user.useEmail}});
    } catch (error) {    
        res.status(500).json({ message: "Erreur serveur lors de la connexion." });
  }
};
