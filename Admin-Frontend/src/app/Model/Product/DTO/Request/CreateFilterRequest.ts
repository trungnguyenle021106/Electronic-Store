import { Filter } from "../../../Filter/Filter";


export interface CreateFilterRequest {
    Filter: Filter;
    productPropertyIDs: number[];
}