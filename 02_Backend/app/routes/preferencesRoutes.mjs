import express from "express";
import { verifyToken } from "../middleware/auth.mjs";
import { getPreferences, updatePreferences } from "../controllers/preferencesController.mjs";

const router = express.Router();

router.get("/", verifyToken, getPreferences);
router.put("/", verifyToken, updatePreferences);

export default router;
