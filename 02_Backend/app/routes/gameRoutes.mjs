import express from "express";
import { verifyToken } from "../middleware/auth.mjs";
import { searchGames, getTrending, getGameDetail } from "../controllers/gameController.mjs";

const router = express.Router();

router.get("/trending", verifyToken, getTrending);
router.get("/search", verifyToken, searchGames);
router.get("/:id", verifyToken, getGameDetail);

export default router;
