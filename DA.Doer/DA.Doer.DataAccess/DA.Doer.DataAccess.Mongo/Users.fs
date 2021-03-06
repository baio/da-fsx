﻿[<AutoOpen>]
module DA.Doer.Mongo.Users

open MongoDB.Bson
open DA.FSX
open FSharpx.Reader
open DA.Doer.Mongo.API
open DA.DataAccess.Domain

// users

let USERS_COLLECTION_NAME = "users"


type UserDocIns = {
    Id: BsonObjectId
    OrgId: string
    Role: string
    FirstName: string
    MidName: string 
    LastName: string
    Email: string
    Phone: string
    Ancestors: string seq
    Avatar: string
}

let createInsDoc (doc: UserDoc) =    
    let id = BsonObjectId(ObjectId.GenerateNewId())
    { 
        Id = id
        OrgId = doc.OrgId
        Role = doc.Role
        FirstName = doc.FirstName
        MidName = doc.MidName
        LastName = doc.LastName
        Email = doc.Email
        Phone = doc.Phone
        Ancestors = doc.Ancestors
        Avatar = doc.Avatar
    }

let createUser doc =    
    let insDoc = createInsDoc doc
    (insert insDoc <!> getCollection USERS_COLLECTION_NAME) |> ReaderTask.mapc(insDoc.Id.AsObjectId.ToString())

let removeUser (id: string) =    
    (remove id <!> getCollection USERS_COLLECTION_NAME) |> ReaderTask.mapc(id)
