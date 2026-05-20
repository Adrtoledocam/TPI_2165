import express from "express";
import { verifyToken } from "../middleware/auth.mjs";
import { getProfile, deleteAccount } from "../controllers/userController.mjs";

const router = express.Router();

router.get("/profile", verifyToken, getProfile);
router.delete("/", verifyToken, deleteAccount);

export default router;
