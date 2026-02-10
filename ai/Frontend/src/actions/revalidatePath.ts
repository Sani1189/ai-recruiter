"use server";

import { revalidatePath as _rp } from "next/cache";

export async function revalidatePath(path: string) {
  _rp(path);
}
