import { Filter } from "../Filter";


export interface CreateUpdateFilterRequest {
    Filter: Filter;
    productPropertyIDs: number[];
}