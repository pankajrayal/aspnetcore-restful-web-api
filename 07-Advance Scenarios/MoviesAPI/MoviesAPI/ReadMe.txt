

// People

GET: https://localhost:44337/api/people/5

POST: https://localhost:44337/api/people
Body:
{
    "name" : "Sushant Singh Rajput",
    "dateOfBirth": "1986-01-21",
    "biography": "Sushant Singh Rajput was an Indian actor best known for his work in Hindi cinema. He starred in a number of commercially successful Bollywood films such as M.S. Dhoni: The Untold Story, Kedarnath and Chhichhore."
}

DEL: https://localhost:44337/api/people/6

PATCH: https://localhost:44337/api/people/5
Body: raw, json
[
    {
        "op" : "replace",
        "path" : "/biography",
        "value": "Jeremy Lee Renner is an American actor and singer. He began his career by appearing in independent films such as Dahmer and Neo Ned. Renner earned supporting roles in bigger films, such as S.W.A.T. and 28 Weeks Later."
    }
]

