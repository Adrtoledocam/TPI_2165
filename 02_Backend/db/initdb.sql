CREATE DATABASE IF NOT EXISTS arcaludo_db;
USE arcaludo_db;

CREATE TABLE t_account (
    accId           INT          AUTO_INCREMENT,
    accUsername     VARCHAR(50)  NOT NULL,
    accEmail        VARCHAR(255) NOT NULL,
    accPasswordHash VARCHAR(60)  NOT NULL,
    accCreatedAt    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (accId),
    UNIQUE (accEmail),
    UNIQUE (accUsername)
) ENGINE=InnoDB;

CREATE TABLE t_preferences_user (
    preId             INT      AUTO_INCREMENT,
    preCommunityOptIn BOOLEAN  NOT NULL DEFAULT FALSE,
    preUpdatedAt      DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
                                        ON UPDATE CURRENT_TIMESTAMP,
    preAccId          INT      NOT NULL,
    PRIMARY KEY (preId),
    UNIQUE (preAccId),
    CONSTRAINT FK_pre_account FOREIGN KEY (preAccId) REFERENCES t_account (accId) ON DELETE CASCADE
) ENGINE=InnoDB;

CREATE TABLE t_game (
    gamId          INT          NOT NULL,
    gamTitle       VARCHAR(255) NOT NULL,
    gamCoverUrl    VARCHAR(500),
    gamPlatforms   VARCHAR(100),
    gamDeveloper   VARCHAR(100),
    gamMetacritic  INT,
    gamPublisher   VARCHAR(100),
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
    colAccId        INT          NOT NULL,
    colGamId        INT,
    PRIMARY KEY (colId),
    CONSTRAINT FK_col_account FOREIGN KEY (colAccId) REFERENCES t_account (accId) ON DELETE CASCADE,
    CONSTRAINT FK_col_game    FOREIGN KEY (colGamId) REFERENCES t_game    (gamId) ON DELETE SET NULL
) ENGINE=InnoDB;
