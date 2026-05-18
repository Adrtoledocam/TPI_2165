import express from "express";
import { verifyToken } from "../middleware/auth.mjs";
import { getCollection, addToCollection, updateCollectionEntry, deleteCollectionEntry} from "../controllers/collectionController.mjs";

const router = express.Router();

// CRUD collection
router.get("/",         verifyToken, getCollection);
router.post("/",        verifyToken, addToCollection);
router.put("/:id",      verifyToken, updateCollectionEntry);
router.delete("/:id",   verifyToken, deleteCollectionEntry);

export default router;
