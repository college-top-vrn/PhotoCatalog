CREATE TABLE photos
(
    id          UUID PRIMARY KEY     DEFAULT uuidv7(),
    user_id     UUID        NOT NULL,
    captured_at TIMESTAMPTZ NOT NULL,
    photo_size  BIGINT      NOT NULL,
    mime        TEXT        NOT NULL,
    storage_key TEXT        NOT NULL,
    metadata    JSONB       NOT NULL DEFAULT '{}'::jsonb,
    CONSTRAINT fk_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT unq_storage_key UNIQUE (storage_key)
);

CREATE INDEX idx_metadata ON photos USING gin (metadata);

CREATE TABLE tags
(
    id       UUID PRIMARY KEY uuidv7(),
    user_id  UUID NOT NULL,
    tag_name TEXT NOT NULL,
    CONSTRAINT fk_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT unq_name UNIQUE (user_id, tag_name)
);

CREATE TABLE photo_tags
(
    tag_id   UUID,
    photo_id UUID,
    PRIMARY KEY (tag_id, photo_id),
    CONSTRAINT fk_tag_id FOREIGN KEY (tag_id) REFERENCES tags (id) ON DELETE RESTRICT,
    CONSTRAINT fk_photo_id FOREIGN KEY (photo_id) REFERENCES photos (id) ON DELETE RESTRICT
);

CREATE TABLE albums
(
    id         UUID PRIMARY KEY uuidv7(),
    user_id    UUID NOT NULL,
    album_name TEXT NOT NULL,
    CONSTRAINT fk_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT unq_name UNIQUE (user_id, album_name)
);

CREATE TABLE album_photos
(
    album_id UUID,
    photo_id UUID,
    PRIMARY KEY (album_id, photo_id),
    CONSTRAINT fk_album_id FOREIGN KEY (album_id) REFERENCES albums (id) ON DELETE RESTRICT,
    CONSTRAINT fk_photo_id FOREIGN KEY (photo_id) REFERENCES photos (id) ON DELETE RESTRICT
);

CREATE INDEX idx_album_photos_photo_id ON album_photos (photo_id);