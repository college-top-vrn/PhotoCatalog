CREATE TABLE photos
(
    id          UUID PRIMARY KEY DEFAULT uuidv7(),
    user_id     UUID    NOT NULL,
    photo_size  INTEGER NOT NULL,
    mime        TEXT    NOT NULL,
    storage_key TEXT    NOT NULL,
    metadata    JSONB   NOT NULL DEFAULT '{}'::jsonb,
    CONSTRAINT fk_user_id FOREIGN KEY (user_id) REFERENCES users (id) ON DELETE RESTRICT,
    CONSTRAINT unq_storage_key UNIQUE (storage_key)
);

CREATE INDEX idx_metadata ON photos USING gin (metadata);