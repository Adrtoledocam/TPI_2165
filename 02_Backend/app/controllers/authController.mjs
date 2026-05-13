import {pool} from "../config/db.mjs";
import bcrypt from "bcryptjs";
import jwt from "jsonwebtoken";

//Créer une compte
export const register = async (req, res) => {
    const {username, email, password} = req.body;
    //Champs reequis
    if (!username || !password || !email)
        return res.status(400).json({error:"Des champs requis sont manquants"});
    //Username min avec 3 caractères et max 50 caractères 
    if(username.length <3 || username.length >50)
        return res.status(400).json({error: "Le nom d'utilisateur doit contenir entre 3 et 50 caractères."})
    //Email avec le format "xxxx@xxx.xxx"
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email))
        return res.status(400).json({error: "Le format de l'adresse email est invalide"})
    //MDP avec securité min 8 caractères, 1 maj, 1 chiffre et 1 caracter special
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[-+_!@#$%^&*.,?]).{8,}$/;    
    if (!passwordRegex.test(password)) 
        return res.status(400).json({ error: "Le mot de passe doit contenir au moins 8 caractères, une majuscule, une minuscule, un chiffre et un symbole."});
    //Verifier si l'email exist
    try{        
        const [existingUser] = await pool.execute('SELECT * FROM t_user WHERE useEmail = ?',[email]);
        if(existingUser.length>0){
            return res.status(400).json({message: "Cet email est déjà utilisé."})
        }
        //MDP hashed
        const salt =  await bcrypt.genSalt(10);
        const hashed = await bcrypt.hash(password, salt);

        const [result] = await pool.execute(
            'INSERT INTO t_user (useUsername, useEmail, usePassword) VALUES (?,?,?)',[username, email, hashed]
        );

        await pool.execute(
                    'INSERT INTO t_preferences_user (preUseId) VALUES (?)',
                    [result.insertId]
                );

        res.json ({message : "Utilisateur créé avec succès !"});
    } catch (err) {
        res.status(500).json({error: "Erreur serveur lors de l'inscription."});
    }
}; 

//Se connecter
export const login = async (req, res) => {
    const {email, password} = req.body;

    if (!email || !password)
        return res.status(400).json({ error: "Email et mot de passe requis." });

    try {
        //Verifier s'il exist l'email
        const [users] = await pool.execute("SELECT * FROM t_user WHERE useEmail = ?", [email]);

        if (users.length === 0)
            return res.status(401).json({ error: "Cet utilisateur n'est pas enregistré" });

        const user = users[0];
        
        if (!user || !(await bcrypt.compare(password, user.usePassword))){
            return res.status(401).json({ message : "Identifiants invalides."});
        }
        
        const token = jwt.sign(
            {
                id: user.useId },
            process.env.JWT_SECRET,
            { expiresIn: "2h" }
        );

        res.json({ token, user: { id: user.useId, username: user.useUsername, email: user.useEmail }});
    } catch (error) {    
        res.status(500).json({ message: "Erreur serveur lors de la connexion." });
  }
};
