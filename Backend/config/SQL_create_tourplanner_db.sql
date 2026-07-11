create table public.users
(
    user_id       BIGINT generated always as identity
        constraint users_pk
            primary key,
    user_name     TEXT not null
        constraint users_unique
            unique,
    password_hash TEXT not null,
    created_at    timestamptz default now(),
    constraint password_hash_length
        check (char_length(users.password_hash) BETWEEN 20 AND 255)
);

create table public.user_sessions
(
    session_id BIGINT generated always as identity
        constraint session_pk
            primary key,
    auth_token TEXT                      not null
        constraint session_token_unique
            unique,
    user_id    BIGINT                   not null
        constraint session_users_fk
            references public.users
            on delete cascade,
    created_at timestamptz not null,
    expires_at timestamptz not null,
    last_seen_at timestamptz
);

create table public.media_entries
(
    media_id              BIGINT generated always as identity
        constraint media_entries_pk
            primary key,
    user_id               BIGINT
        constraint fk_media_user
            references public.users (user_id)
            on delete set null,
    media_title           TEXT    not null,
    media_description     TEXT,
    media_type            INTEGER not null,
    media_release_year    INTEGER,
    media_age_restriction INTEGER default 0,
    media_guid uuid default gen_random_uuid() not null
        constraint media_entries_pk_2
        unique,
    constraint age_restriction_check
        check (media_age_restriction >= 0),
    constraint release_year_check
        check (media_entries.media_release_year >= 1878)
);

create table public.media_genre
(
    genre_id BIGINT generated always as identity
        constraint media_genre_pk primary key,
    genre_name TEXT not null
        constraint media_name_unique
            unique
);

INSERT INTO public.media_genre (genre_name)
VALUES
    ('Action'),
    ('Sci-Fi'),
    ('Comedy'),
    ('Drama'),
    ('Horror'),
    ('Documentary'),
    ('Thriller');

create table public.media_genre_mapping
(
    media_id BIGINT not null
        constraint fk_mapping_media
            references public.media_entries (media_id)
            on delete cascade,
    genre_id BIGINT not null
        constraint  fk_mapping_genre
            references public.media_genre (genre_id)
            on delete cascade,
    constraint media_genre_mapping_pk
        primary key (media_id, genre_id)
);

-- 1. RATINGS TABLE (Supports Stars, Comments, and Moderation)
create table public.ratings
(
    rating_id     BIGINT generated always as identity
        constraint ratings_pk
            primary key,
    media_id      BIGINT not null
        constraint fk_ratings_media
            references public.media_entries (media_id)
            on delete cascade,
    user_id       BIGINT not null
        constraint fk_ratings_user
            references public.users (user_id)
            on delete cascade,
    rating_guid  uuid not null DEFAULT gen_random_uuid() NOT NULL
        CONSTRAINT ratings_guid_unique
            unique,
    star_rating   INTEGER not null
        constraint check_stars
            check (star_rating between 1 and 5),
    comment_text  TEXT,

    -- MODERATION FEATURE:
    -- If false, comment is hidden from public.
    -- Only Media Creator sees it to approve it.
    is_approved   BOOLEAN default false not null,

    created_at    timestamptz default now() not null,

    -- Ensure a user can only rate a movie ONCE
    constraint unique_user_rating_per_media
        unique (media_id, user_id)
);

-- 2. FAVORITES TABLE (Simple Bookmark)
create table public.favorites
(
    user_id  BIGINT not null
        constraint fk_fav_user
            references public.users (user_id)
            on delete cascade,
    media_id BIGINT not null
        constraint fk_fav_media
            references public.media_entries (media_id)
            on delete cascade,
    created_at timestamptz default now(),
    constraint favorites_pk
        primary key (user_id, media_id)
);

-- 3. RATING LIKES (Users liking other reviews)
create table public.rating_likes
(
    rating_id BIGINT not null
        constraint fk_like_rating
            references public.ratings (rating_id)
            on delete cascade,
    user_id   BIGINT not null
        constraint fk_like_user
            references public.users (user_id)
            on delete cascade,
    constraint rating_likes_pk
        primary key (rating_id, user_id) -- One like per user per rating
);
