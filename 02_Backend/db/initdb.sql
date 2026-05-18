CREATE DATABASE IF NOT EXISTS arcaludo_db;
USE arcaludo_db;

CREATE TABLE t_user (
    useId           INT          AUTO_INCREMENT,
    useUsername     VARCHAR(50)  NOT NULL,
    useEmail        VARCHAR(255) NOT NULL,
    usePasswordHash VARCHAR(60)  NOT NULL,
    useCreatedAt    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (useId),
    UNIQUE (useEmail),
    UNIQUE (useUsername)
) ENGINE=InnoDB;

CREATE TABLE t_preferences_user (
    preId             INT      AUTO_INCREMENT,
    preCommunityOptIn BOOLEAN  NOT NULL DEFAULT FALSE,
    preUpdatedAt      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    preUseId          INT      NOT NULL,
    PRIMARY KEY (preId),
    UNIQUE (preUseId),
    CONSTRAINT FK_pre_user FOREIGN KEY (preUseId) REFERENCES t_user (useId) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE t_game (
    gamId          INT          NOT NULL,
    gamTitle       VARCHAR(255) NOT NULL,
    gamCoverUrl    VARCHAR(500),
    gamPlatforms   VARCHAR(100),
    gamDeveloper   VARCHAR(100),
    gamPublisher   VARCHAR(100),
    gamMetacritic  INT,
    gamGenre       VARCHAR(100),
    gamReleaseDate DATE,
    PRIMARY KEY (gamId)
) ENGINE=InnoDB;

CREATE TABLE t_collection_game (
    colId           INT          AUTO_INCREMENT,
    colStatus       VARCHAR(20)  NOT NULL,
    colRating       SMALLINT,
    colComment      TEXT,
    colPlaytime     INT,
    colOwnPlatforms VARCHAR(100),
    colAddedAt      DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    colUseId        INT          NOT NULL,
    colGamId        INT,
    PRIMARY KEY (colId),
    CONSTRAINT FK_col_user FOREIGN KEY (colUseId) REFERENCES t_user (useId) ON DELETE CASCADE,
    CONSTRAINT FK_col_game FOREIGN KEY (colGamId) REFERENCES t_game (gamId) ON DELETE SET NULL
) ENGINE=InnoDB;
